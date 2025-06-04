import React, { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import axios from "axios";

interface User {
  id: string;
  name: string;
}

interface Message {
  id: string;
  senderId: string;
  content: string;
  timestamp: string;
  receiverId?: string;
  chatRoomId?: string;
}
// interfaces.ts
export interface ChatRoomToAddDTO {
  name: string;
  userIds: string[]; // Using string instead of Guid to match frontend
}

export interface ChatRoomToReturnDTO {
  id: string;
  name: string;
  userIds: string[];
  createdAt?: string;
  messages: Message[];
}

export interface PagedResponse<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AddUserToRoomDTO {
  chatRoomId: string; // Using string instead of Guid
  userIds: string[]; // Using string[] instead of Guid[]
}

export interface ChatRoom extends ChatRoomToReturnDTO {}
// api/chatRoomService.ts

const API_URL = "http://localhost:5032";
export const createChatRoom = async (
  roomData: ChatRoomToAddDTO
): Promise<ChatRoomToReturnDTO> => {
  try {
    const response = await axios.post<ChatRoomToReturnDTO>(
      `${API_URL}/chatrooms`,
      roomData,
      {
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error("API Error:", error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || "Failed to create room");
    }
    throw new Error("Network error occurred");
  }
};

export const getUserGroups = async (
  userId: string,
  pageIndex: number = 0,
  pageSize: number = 10
): Promise<PagedResponse<ChatRoomToReturnDTO>> => {
  try {
    const response = await axios.get<PagedResponse<ChatRoomToReturnDTO>>(
      `${API_URL}/chat/groups/${userId}`,
      {
        params: {
          pageIndex,
          pageSize,
        },
      }
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error("API Error:", error.response?.status, error.response?.data);
      throw new Error(
        error.response?.data?.message || "Failed to fetch user groups"
      );
    }
    throw new Error("Network error occurred");
  }
};

// api/chatRoomService.ts
export const addUsersToRoom = async (dto: AddUserToRoomDTO): Promise<void> => {
  try {
    await axios.post(`${API_URL}/chatrooms/add-users`, dto, {
      headers: {
        "Content-Type": "application/json",
      },
    });
  } catch (error) {
    if (axios.isAxiosError(error)) {
      console.error("API Error:", error.response?.status, error.response?.data);
      throw new Error(
        error.response?.data?.message || "Failed to add users to room"
      );
    }
    throw new Error("Network error occurred");
  }
};

const ChatApp: React.FC = () => {
  // Connection state
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [isConnected, setIsConnected] = useState(false);

  // User and room state
  const [currentUser, setCurrentUser] = useState<User>({
    id: "169ea051-e369-43d7-9494-31c32b16ae1d",
    name: "Current User",
  });

  const [targetUser, setTargetUser] = useState<User>({
    id: "a89c71ce-f4d4-4a0f-87ff-e7de2cb1223b",
    name: "Target User",
  });

  const [currentRoom, setCurrentRoom] = useState<ChatRoom>(
    {} as ChatRoomToReturnDTO
  );
  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize: 10,
    totalCount: 0,
  });

  const [availableRooms, setAvailableRooms] = useState<ChatRoom[]>([]);
  const [usersInRoom, setUsersInRoom] = useState<User[]>([]);
  const [availableUsers, setAvailableUsers] = useState<User[]>([]);
  const [newRoomName, setNewRoomName] = useState("");
  const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
  const [selectedRoomId, setSelectedRoomId] = useState<string>("");

  // Message state
  const [messageInput, setMessageInput] = useState("");
  const [messages, setMessages] = useState<Message[]>([]);
  const [userToAdd, setUserToAdd] = useState("");

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // Initialize SignalR connection
  useEffect(() => {
    const createConnection = async () => {
      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5032/chatHub")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

      try {
        await newConnection.start();
        console.log("SignalR Connected");

        connectionRef.current = newConnection;
        setConnection(newConnection);
        setIsConnected(true);

        setupListeners(newConnection);

        // Initial data loading
        await loadRooms();
        await loadUsers();
      } catch (error) {
        console.error("Connection failed: ", error);
      }
    };

    createConnection();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, []);

  // Replace your existing loadRooms function with:

  const loadUserGroups = async () => {
    try {
      const response = await getUserGroups(
        currentUser.id,
        pagination.pageIndex,
        pagination.pageSize
      );

      setAvailableRooms(response.items);
      setPagination((prev) => ({
        ...prev,
        totalCount: response.totalCount,
      }));

      if (response.items.length > 0 && !currentRoom.id) {
        setCurrentRoom(response.items[0]);
      }
    } catch (error) {
      console.error("Failed to load user groups:", error);
    }
  };

  // Add pagination controls in your component
  const handlePageChange = (newPageIndex: number) => {
    setPagination((prev) => ({ ...prev, pageIndex: newPageIndex }));
  };

  useEffect(() => {
    loadUserGroups();
  }, [currentUser.id, pagination.pageIndex, pagination.pageSize]);

  const handleCreateRoom = async () => {
    if (!newRoomName.trim()) return;

    setIsLoading(true);
    setError(null);

    try {
      const newRoom: ChatRoomToAddDTO = {
        name: newRoomName,
        userIds: [currentUser.id, ...selectedUsers], // Include current user and selected users
      };

      const createdRoom = await createChatRoom(newRoom);

      // Update local state
      setAvailableRooms((prev) => [...prev, createdRoom]);
      setCurrentRoom(createdRoom);
      setNewRoomName("");
      setSelectedUsers([]);

      // Join the SignalR group
      if (connectionRef.current) {
        await connectionRef.current.invoke(
          "AddToRoom",
          createdRoom.id,
          currentUser.id
        );
      }
    } catch (error) {
      setError("Failed to create room. Please try again.");
      console.error("Failed to create room:", error);
    } finally {
      setIsLoading(false);
    }
  };
  {
    error && <div className="error-message">{error}</div>;
  }
  {
    isLoading && <div>Creating room...</div>;
  }

  // Add this to your useEffect for initial load
  useEffect(() => {
    const initialize = async () => {
      await loadUserGroups(); // Changed from loadRooms
      await loadUsers();
    };
    initialize();
  }, [currentUser.id]);

  const loadUsers = async () => {
    // In a real app, this would come from your API
    setAvailableUsers([
      { id: "a89c71ce-f4d4-4a0f-87ff-e7de2cb1223b", name: "Target User" },
      { id: "b89c71ce-f4d4-4a0f-87ff-e7de2cb1223b", name: "Another User" },
    ]);
  };

  // Setup all SignalR listeners
  const setupListeners = (hubConnection: signalR.HubConnection) => {
    hubConnection.on(
      "ReceiveMessage",
      (data: { Message: string; Sender: string; Timestamp: string }) => {
        const newMessage: Message = {
          id: Date.now().toString(),
          senderId: data.Sender,
          content: data.Message,
          timestamp: data.Timestamp,
          receiverId: currentUser.id, // Since this is a private message to us
        };

        setMessages((prev) => [...prev, newMessage]);
      }
    );

    // Handle group messages
    hubConnection.on(
      "ReceiveGroupMessage",
      (data: {
        Message: string;
        Sender: string;
        RoomId: string;
        Timestamp: string;
      }) => {
        const newMessage: Message = {
          id: Date.now().toString(),
          senderId: data.Sender,
          content: data.Message,
          timestamp: data.Timestamp,
          chatRoomId: data.RoomId,
        };

        setMessages((prev) => [...prev, newMessage]);
      }
    );

    hubConnection.on("UserAddedToRoom", (roomId: string, userIds: string[]) => {
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `Users ${userIds.join(", ")} added to room ${roomId}`,
          timestamp: new Date().toISOString(),
        },
      ]);
    });

    hubConnection.on("UserLeftRoom", (roomId: string, userId: string) => {
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `User ${userId} left room ${roomId}`,
          timestamp: new Date().toISOString(),
        },
      ]);
    });

    hubConnection.on("UserKickedFromRoom", (roomId: string, userId: string) => {
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `User ${userId} was kicked from room ${roomId}`,
          timestamp: new Date().toISOString(),
        },
      ]);
    });
    hubConnection.on("UserAddedToRoom", (roomId: string, userIds: string[]) => {
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `Users ${userIds.join(", ")} added to room`,
          timestamp: new Date().toISOString(),
          chatRoomId: roomId,
        },
      ]);

      // Update local state if needed
      if (roomId === currentRoom.id) {
        setUsersInRoom((prev) => [
          ...prev,
          ...(userIds
            .map((id) => availableUsers.find((u) => u.id === id))
            .filter(Boolean) as User[]),
        ]);
      }
    });
  };
  const loadRooms = async () => {
    try {
      // Pass the current user's ID and default pagination parameters
      const response = await getUserGroups(
        currentUser.id, // Required userId parameter
        0, // Optional pageIndex (default to 0)
        10 // Optional pageSize (default to 10)
      );

      // Use response.items instead of the full response
      setAvailableRooms(response.items);

      // Auto-join the first room if none is selected
      if (response.items.length > 0 && !currentRoom.id) {
        setCurrentRoom(response.items[0]);
      }
    } catch (error) {
      console.error("Failed to load rooms:", error);
    }
  };
  const loadChatRoom = async () => {
    try {
      const res = await axios.get<ChatRoomToReturnDTO>(`${API_URL}/chatrooms`, {
        params: { roomId: selectedRoomId },
      });
      setMessages(res.data.messages); // 👈 use messages
    } catch (error) {
      console.error("Failed to load chat room:", error);
    }
  };

  if (selectedRoomId) {
    loadChatRoom();
  }

  // Add this to your useEffect for initial load
  useEffect(() => {
    const initialize = async () => {
      await loadRooms();
      await loadUsers();
    };
    initialize();
  }, []);
  // Room management functions

  const joinRoom = async (roomId: string) => {
    if (!connectionRef.current) return;

    try {
      // Find the selected room
      const room = availableRooms.find((r) => r.id === roomId);
      if (!room) {
        console.error("Room not found in availableRooms.");
        return;
      }

      // Fetch full room details from backend (with messages)
      const response = await axios.get<ChatRoomToReturnDTO>(
        `${API_URL}/chatrooms`,
        { params: { roomId } }
      );

      const fullRoom = response.data;

      // Set the room and its messages
      setCurrentRoom(fullRoom);
      setMessages([
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `You joined ${fullRoom.name}`,
          timestamp: new Date().toISOString(),
        },
        ...fullRoom.messages,
      ]);

      // Optionally notify server via SignalR
      await connectionRef.current.invoke("JoinRoom", roomId);
    } catch (error) {
      console.error("Failed to join room:", error);
    }
  };

  const leaveRoom = async () => {
    if (!connectionRef.current) return;

    try {
      await connectionRef.current.invoke(
        "LeaveRoom",
        currentRoom.id,
        currentUser.id
      );
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `You left ${currentRoom.name}`,
          timestamp: new Date().toISOString(),
        },
      ]);
    } catch (error) {
      console.error("Failed to leave room:", error);
    }
  };

  const addUserToRoom = async (userId: string) => {
    if (!connectionRef.current || !currentRoom.id) return;

    try {
      const dto: AddUserToRoomDTO = {
        chatRoomId: currentRoom.id,
        userIds: [userId], // Convert to array even for single user
      };

      // Call the API directly
      await addUsersToRoom(dto);

      // Also notify via SignalR if needed
      if (connectionRef.current) {
        await connectionRef.current.invoke("AddToRoom", dto);
      }

      // Update local state
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          senderId: "system",
          content: `User ${userId} added to room`,
          timestamp: new Date().toISOString(),
          chatRoomId: currentRoom.id,
        },
      ]);
    } catch (error) {
      console.error("Failed to add user to room:", error);
    }
  };

  const kickUserFromRoom = async (userId: string) => {
    if (!connectionRef.current) return;

    try {
      await connectionRef.current.invoke(
        "KickUserFromRoom",
        currentRoom.id,
        userId
      );
    } catch (error) {
      console.error("Failed to kick user from room:", error);
    }
  };

  // Message sending functions
  const sendPrivateMessage = async () => {
    if (!connectionRef.current || !messageInput.trim()) return;

    const message: MessageToSendDTO = {
      SenderId: currentUser.id,
      ReceiverId: targetUser.id,
      Content: messageInput,
    };

    try {
      // Optimistic UI update
      const newMessage: Message = {
        id: Date.now().toString(),
        senderId: currentUser.id,
        receiverId: targetUser.id,
        content: messageInput,
        timestamp: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, newMessage]);
      setMessageInput("");

      await connectionRef.current.invoke("SendMessage", message);
    } catch (error) {
      console.error("Failed to send private message:", error);
    }
  };

  const sendGroupMessage = async () => {
    if (!connectionRef.current || !messageInput.trim()) return;

    const message: MessageToSendDTO = {
      SenderId: currentUser.id,
      ChatRoomId: currentRoom.id,
      Content: messageInput,
    };

    try {
      // Optimistic UI update
      const newMessage: Message = {
        id: Date.now().toString(),
        senderId: currentUser.id,
        chatRoomId: currentRoom.id,
        content: messageInput,
        timestamp: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, newMessage]);
      setMessageInput("");

      await connectionRef.current.invoke("SendMessage", message);
    } catch (error) {
      console.error("Failed to send group message:", error);
    }
  };
  return (
    <div className="chat-app">
      <h2>Chat Application</h2>

      <div className="connection-status">
        Status: {isConnected ? "Connected" : "Disconnected"}
      </div>

      <div className="user-controls">
        <h3>User Settings</h3>
        <div>
          <label>Your Name: </label>
          <input
            value={currentUser.name}
            onChange={(e) =>
              setCurrentUser({ ...currentUser, name: e.target.value })
            }
          />
        </div>
        <div>
          <label>Target User: </label>
          <select
            value={targetUser.id}
            onChange={(e) => {
              const user = availableUsers.find((u) => u.id === e.target.value);
              if (user) setTargetUser(user);
            }}
          >
            {availableUsers.map((user) => (
              <option key={user.id} value={user.id}>
                {user.name}
              </option>
            ))}
          </select>
        </div>
      </div>
      <div className="room-creation">
        <h3>Create New Chat Room</h3>
        <div>
          <input
            type="text"
            placeholder="Room name"
            value={newRoomName}
            onChange={(e) => setNewRoomName(e.target.value)}
          />
        </div>

        <div>
          <h4>Add Users to Room</h4>
          <select
            multiple
            value={selectedUsers}
            onChange={(e) => {
              const options = Array.from(
                e.target.selectedOptions,
                (option) => option.value
              );
              setSelectedUsers(options);
            }}
          >
            {availableUsers
              .filter((user) => user.id !== currentUser.id)
              .map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name}
                </option>
              ))}
          </select>
        </div>

        <button onClick={handleCreateRoom} disabled={!newRoomName.trim()}>
          Create Room
        </button>
      </div>
      <div className="room-controls">
        <h3>Room Management</h3>
        <div>
          <label>Current Room: </label>
          <select
            value={currentRoom.id}
            onChange={(e) => {
              const room = availableRooms.find((r) => r.id === e.target.value);
              if (room) joinRoom(room.id);
            }}
          >
            {availableRooms.map((room) => (
              <option key={room.id} value={room.id}>
                {room.name}
              </option>
            ))}
          </select>
          <button onClick={leaveRoom}>Leave Room</button>
        </div>

        <div>
          <h4>Add User to Room</h4>
          <select
            value={userToAdd}
            onChange={(e) => setUserToAdd(e.target.value)}
          >
            <option value="">Select a user</option>
            {availableUsers
              .filter((user) => user.id !== currentUser.id)
              .map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name}
                </option>
              ))}
          </select>
          <button
            onClick={() => userToAdd && addUserToRoom(userToAdd)}
            disabled={!userToAdd}
          >
            Add User
          </button>
        </div>

        <div>
          <h4>Users in Room</h4>
          <ul>
            {usersInRoom.map((user) => (
              <li key={user.id}>
                {user.name}
                {user.id !== currentUser.id && (
                  <button onClick={() => kickUserFromRoom(user.id)}>
                    Kick
                  </button>
                )}
              </li>
            ))}
          </ul>
        </div>
      </div>

      <div className="message-input">
        <h3>Send Message</h3>
        <textarea
          value={messageInput}
          onChange={(e) => setMessageInput(e.target.value)}
          placeholder="Type your message here..."
        />
        <div>
          <button onClick={sendPrivateMessage}>Send Private</button>
          <button onClick={sendGroupMessage}>Send to Group</button>
        </div>
      </div>

      <div className="message-display">
        <h3>Messages</h3>
        <div className="messages-container">
          {messages.map((message) => {
            const isCurrentUser = message.senderId === currentUser.id;
            const senderName = isCurrentUser
              ? "You"
              : availableUsers.find((u) => u.id === message.senderId)?.name ||
                message.senderId;

            return (
              <div
                key={message.id}
                className={`message ${
                  isCurrentUser ? "current-user" : "other-user"
                } ${message.receiverId ? "private-message" : "group-message"}`}
              >
                <div className="message-header">
                  <strong>
                    {senderName} (ID: {message.senderId})
                  </strong>
                  <span>
                    {new Date(message.timestamp).toLocaleTimeString()}
                  </span>
                </div>
                <div className="message-content">{message.content}</div>
                {message.receiverId && (
                  <div className="message-info">
                    {isCurrentUser ? "To: " : "From: "}
                    {availableUsers.find(
                      (u) =>
                        u.id ===
                        (isCurrentUser ? message.receiverId : message.senderId)
                    )?.name ||
                      (isCurrentUser ? message.receiverId : message.senderId)}
                  </div>
                )}
                {message.chatRoomId && (
                  <div className="message-info">
                    Room:{" "}
                    {availableRooms.find((r) => r.id === message.chatRoomId)
                      ?.name || message.chatRoomId}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
      <div className="pagination-controls">
        <button
          onClick={() => handlePageChange(pagination.pageIndex - 1)}
          disabled={pagination.pageIndex === 0}
        >
          Previous
        </button>

        <span>
          Page {pagination.pageIndex + 1} of{" "}
          {Math.ceil(pagination.totalCount / pagination.pageSize)}
        </span>

        <button
          onClick={() => handlePageChange(pagination.pageIndex + 1)}
          disabled={
            (pagination.pageIndex + 1) * pagination.pageSize >=
            pagination.totalCount
          }
        >
          Next
        </button>

        <select
          value={pagination.pageSize}
          onChange={(e) => {
            setPagination((prev) => ({
              ...prev,
              pageSize: Number(e.target.value),
              pageIndex: 0, // Reset to first page when changing page size
            }));
          }}
        >
          <option value="5">5 per page</option>
          <option value="10">10 per page</option>
          <option value="20">20 per page</option>
          <option value="50">50 per page</option>
        </select>
      </div>
    </div>
  );
};

export default ChatApp;

// Interface to match your backend DTO
interface MessageToSendDTO {
  SenderId: string;
  ReceiverId?: string;
  ChatRoomId?: string;
  Content: string;
}
