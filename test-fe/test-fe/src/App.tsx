import React, { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";

const App: React.FC = () => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [message, setMessage] = useState("");
  const [messages, setMessages] = useState<string[]>([]);
  const [groupMessages, setGroupMessages] = useState<Record<string, string[]>>(
    {}
  );

  const [userId, setUserId] = useState("169ea051-e369-43d7-9494-31c32b16ae1d"); // your own ID
  const [receiverId, setReceiverId] = useState(
    "a89c71ce-f4d4-4a0f-87ff-e7de2cb1223b"
  ); // for 1-1 chat
  const [chatRoom, setChatRoom] = useState("room1"); // for group chat

  const connectedRef = useRef(false);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5032/chatHub")
      .withAutomaticReconnect()
      .build();

    newConnection
      .start()
      .then(() => {
        console.log("SignalR Connected.");
        connectedRef.current = true;

        // Listeners
        newConnection.on("ReceiveMessage", (msg) => {
          setMessages((prev) => [...prev, `[Private] ${msg.content}`]);
        });

        newConnection.on("ReceiveGroupMessage", (msg) => {
          const group = msg.chatRoom || "default";
          setGroupMessages((prev) => {
            const current = prev[group] || [];
            return {
              ...prev,
              [group]: [...current, `[${msg.senderId}] ${msg.content}`],
            };
          });
        });

        // Auto join a room
        newConnection.invoke("JoinRoom", chatRoom);
      })
      .catch((e) => console.error("Connection failed: ", e));

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, [chatRoom]);

  const sendMessageToUser = async () => {
    if (connection && connectedRef.current) {
      const dto = {
        senderId: userId,
        receiverId: receiverId,
        content: message,
      };
      await connection.invoke("SendMessageToUser", receiverId, dto);
      setMessages((prev) => [...prev, `[To ${receiverId}] ${message}`]);
      setMessage("");
    }
  };

  const sendMessageToGroup = async () => {
    if (connection && connectedRef.current) {
      const dto = {
        senderId: userId,
        chatRoom: chatRoom,
        content: message,
      };
      await connection.invoke("SendMessageToGroup", chatRoom, dto);

      setGroupMessages((prev) => {
        const current = prev[chatRoom] || [];
        return {
          ...prev,
          [chatRoom]: [...current, `[You] ${message}`],
        };
      });
    }
  };

  const joinRoom = async () => {
    if (connection && connectedRef.current) {
      await connection.invoke("JoinRoom", chatRoom);
      setMessages((prev) => [...prev, `You joined ${chatRoom}`]);
    }
  };

  return (
    <div style={{ padding: 20 }}>
      <h2>SignalR Chat (TSX)</h2>

      <div>
        <label>Your User ID: </label>
        <input value={userId} onChange={(e) => setUserId(e.target.value)} />
      </div>
      <div>
        <label>Receiver ID (1-1 chat): </label>
        <input
          value={receiverId}
          onChange={(e) => setReceiverId(e.target.value)}
        />
      </div>
      <div>
        <label>Chat Room: </label>
        <input value={chatRoom} onChange={(e) => setChatRoom(e.target.value)} />
        <button onClick={joinRoom}>Join Room</button>
      </div>

      <div>
        <textarea
          rows={3}
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          placeholder="Type message"
        />
        <div>
          <button onClick={sendMessageToUser}>Send Private</button>
          <button onClick={sendMessageToGroup}>Send to Group</button>
        </div>
      </div>

      <hr />
      <div>
        <h3>Messages</h3>
        {messages.map((m, i) => (
          <div key={i}>{m}</div>
        ))}
        <h3>Group Messages for: {chatRoom}</h3>
        <div
          style={{
            border: "1px solid gray",
            padding: "10px",
            minHeight: "150px",
          }}
        >
          {groupMessages[chatRoom]?.map((m, i) => <div key={i}>{m}</div>) ?? (
            <p>No messages in this group.</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default App;
