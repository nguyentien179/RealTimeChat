using System;
using messaging.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace messaging.Persistence.DataConfig;

public class ChatRoomUserConfiguration : IEntityTypeConfiguration<ChatRoomUser>
{
    public void Configure(EntityTypeBuilder<ChatRoomUser> builder)
    {
        builder.HasKey(c => new { c.ChatRoomId, c.UserId });

        builder
            .HasOne(c => c.ChatRoom)
            .WithMany(r => r.Users)
            .HasForeignKey(c => c.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
