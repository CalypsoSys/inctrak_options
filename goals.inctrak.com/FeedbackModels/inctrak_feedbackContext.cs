using System;
using IncTrak.GoalSetter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IncTrak.GoalSetter.FeedbackModels
{
    public partial class inctrak_feedbackContext : DbContext
    {
        private AppSettings _settings;

        public inctrak_feedbackContext(AppSettings settings)
        {
            _settings = settings;
        }

        public inctrak_feedbackContext(DbContextOptions<inctrak_feedbackContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<MessageType> MessageType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseNpgsql(_settings.FeedbackConnection);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => e.FeedbackPk)
                    .HasName("feedback_pkey");

                entity.ToTable("feedback");

                entity.Property(e => e.FeedbackPk).HasColumnName("feedback_pk");

                entity.Property(e => e.ClientData).HasColumnName("client_data");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasColumnName("email_address")
                    .HasMaxLength(128);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnName("message");

                entity.Property(e => e.MessageTypeFk).HasColumnName("message_type_fk");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnName("subject")
                    .HasMaxLength(50);

                entity.HasOne(d => d.MessageTypeFkNavigation)
                    .WithMany(p => p.Feedback)
                    .HasForeignKey(d => d.MessageTypeFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("feedback_message_type_fk_fkey");
            });

            modelBuilder.Entity<MessageType>(entity =>
            {
                entity.HasKey(e => e.MessageTypePk)
                    .HasName("message_type_pkey");

                entity.ToTable("message_type");

                entity.Property(e => e.MessageTypePk)
                    .HasColumnName("message_type_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.MessageType1)
                    .IsRequired()
                    .HasColumnName("message_type")
                    .HasMaxLength(30);
            });
        }
    }
}
