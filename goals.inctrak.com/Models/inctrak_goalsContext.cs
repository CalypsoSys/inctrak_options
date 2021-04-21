using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IncTrak.GoalSetter.Models
{
    public partial class inctrak_goalsContext : DbContext
    {
        public inctrak_goalsContext()
        {
        }

        public inctrak_goalsContext(DbContextOptions<inctrak_goalsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActivateUuids> ActivateUuids { get; set; }
        public virtual DbSet<Departments> Departments { get; set; }
        public virtual DbSet<Goals> Goals { get; set; }
        public virtual DbSet<Goalset> Goalset { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<ImportanceType> ImportanceType { get; set; }
        public virtual DbSet<LoggedActions> LoggedActions { get; set; }
        public virtual DbSet<Members> Members { get; set; }
        public virtual DbSet<RatingType> RatingType { get; set; }
        public virtual DbSet<Schedules> Schedules { get; set; }
        public virtual DbSet<Teams> Teams { get; set; }
        public virtual DbSet<Users> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("hstore")
                .HasPostgresExtension("uuid-ossp")
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<ActivateUuids>(entity =>
            {
                entity.HasKey(e => e.UuidPk)
                    .HasName("activate_uuids_pkey");

                entity.ToTable("activate_uuids");

                entity.HasIndex(e => e.Uuid)
                    .HasName("activate_uuids_uuid_key")
                    .IsUnique();

                entity.HasIndex(e => new { e.Type, e.UserFk })
                    .HasName("activate_uuids_TYPE_user_fk_key")
                    .IsUnique();

                entity.Property(e => e.UuidPk).HasColumnName("uuid_pk");

                entity.Property(e => e.Type).HasColumnName("TYPE");

                entity.Property(e => e.UserFk).HasColumnName("user_fk");

                entity.Property(e => e.Uuid)
                    .IsRequired()
                    .HasColumnName("uuid")
                    .HasMaxLength(32);

                entity.Property(e => e.ValidUntil)
                    .HasColumnName("valid_until")
                    .HasColumnType("timestamp with time zone");

                entity.HasOne(d => d.UserFkNavigation)
                    .WithMany(p => p.ActivateUuids)
                    .HasForeignKey(d => d.UserFk)
                    .HasConstraintName("activate_uuids_user_fk_fkey");
            });

            modelBuilder.Entity<Departments>(entity =>
            {
                entity.HasKey(e => e.DepartmentPk)
                    .HasName("departments_pkey");

                entity.ToTable("departments");

                entity.Property(e => e.DepartmentPk)
                    .HasColumnName("department_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Departments)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("departments_group_fk_fkey");
            });

            modelBuilder.Entity<Goals>(entity =>
            {
                entity.HasKey(e => e.GoalsPk)
                    .HasName("goals_pkey");

                entity.ToTable("goals");

                entity.Property(e => e.GoalsPk)
                    .HasColumnName("goals_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description");

                entity.Property(e => e.GoalsetFk).HasColumnName("goalset_fk");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.ImportanceTypeFk).HasColumnName("importance_type_fk");

                entity.Property(e => e.ManagerComments).HasColumnName("manager_comments");

                entity.Property(e => e.MemberComments).HasColumnName("member_comments");

                entity.Property(e => e.RatingTypeFk).HasColumnName("rating_type_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GoalsetFkNavigation)
                    .WithMany(p => p.Goals)
                    .HasForeignKey(d => d.GoalsetFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goals_goalset_fk_fkey");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Goals)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goals_group_fk_fkey");

                entity.HasOne(d => d.ImportanceTypeFkNavigation)
                    .WithMany(p => p.Goals)
                    .HasForeignKey(d => d.ImportanceTypeFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goals_importance_type_fk_fkey");

                entity.HasOne(d => d.RatingTypeFkNavigation)
                    .WithMany(p => p.Goals)
                    .HasForeignKey(d => d.RatingTypeFk)
                    .HasConstraintName("goals_rating_type_fk_fkey");
            });

            modelBuilder.Entity<Goalset>(entity =>
            {
                entity.HasKey(e => e.GoalsetPk)
                    .HasName("goalset_pkey");

                entity.ToTable("goalset");

                entity.HasIndex(e => new { e.MemberFk, e.ScheduleFk })
                    .HasName("uc_goalset_member_sched")
                    .IsUnique();

                entity.Property(e => e.GoalsetPk)
                    .HasColumnName("goalset_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.MemberFk).HasColumnName("member_fk");

                entity.Property(e => e.ScheduleFk).HasColumnName("schedule_fk");

                entity.Property(e => e.TeamFk).HasColumnName("team_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Goalset)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goalset_group_fk_fkey");

                entity.HasOne(d => d.MemberFkNavigation)
                    .WithMany(p => p.Goalset)
                    .HasForeignKey(d => d.MemberFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goalset_member_fk_fkey");

                entity.HasOne(d => d.ScheduleFkNavigation)
                    .WithMany(p => p.Goalset)
                    .HasForeignKey(d => d.ScheduleFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goalset_schedule_fk_fkey");

                entity.HasOne(d => d.TeamFkNavigation)
                    .WithMany(p => p.Goalset)
                    .HasForeignKey(d => d.TeamFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("goalset_team_fk_fkey");
            });

            modelBuilder.Entity<Groups>(entity =>
            {
                entity.HasKey(e => e.GroupPk)
                    .HasName("groups_pkey");

                entity.ToTable("groups");

                entity.HasIndex(e => e.GroupKey)
                    .HasName("uc_group_key")
                    .IsUnique();

                entity.Property(e => e.GroupPk)
                    .HasColumnName("group_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(256);

                entity.Property(e => e.GroupKey)
                    .IsRequired()
                    .HasColumnName("group_key")
                    .HasMaxLength(256);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<ImportanceType>(entity =>
            {
                entity.HasKey(e => e.ImportanceTypePk)
                    .HasName("importance_type_pkey");

                entity.ToTable("importance_type");

                entity.Property(e => e.ImportanceTypePk)
                    .HasColumnName("importance_type_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<LoggedActions>(entity =>
            {
                entity.HasKey(e => e.LoggedActionsPk)
                    .HasName("logged_actions_pkey");

                entity.ToTable("logged_actions");

                entity.HasIndex(e => e.Created)
                    .HasName("idx_logged_created");

                entity.HasIndex(e => e.TableName)
                    .HasName("idx_logged_actions_table_name");

                entity.Property(e => e.LoggedActionsPk).HasColumnName("logged_actions_pk");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasColumnName("action");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.NewData).HasColumnName("new_data");

                entity.Property(e => e.OriginalData).HasColumnName("original_data");

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasColumnName("table_name");

                entity.Property(e => e.UserFk).HasColumnName("user_fk");

                entity.Property(e => e.UserName).HasColumnName("user_name");
            });

            modelBuilder.Entity<Members>(entity =>
            {
                entity.HasKey(e => e.MemberPk)
                    .HasName("members_pkey");

                entity.ToTable("members");

                entity.Property(e => e.MemberPk)
                    .HasColumnName("member_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.UserFk).HasColumnName("user_fk");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Members)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("members_group_fk_fkey");

                entity.HasOne(d => d.UserFkNavigation)
                    .WithMany(p => p.Members)
                    .HasForeignKey(d => d.UserFk)
                    .HasConstraintName("members_user_fk_fkey");
            });

            modelBuilder.Entity<RatingType>(entity =>
            {
                entity.HasKey(e => e.RatingTypePk)
                    .HasName("rating_type_pkey");

                entity.ToTable("rating_type");

                entity.Property(e => e.RatingTypePk)
                    .HasColumnName("rating_type_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Schedules>(entity =>
            {
                entity.HasKey(e => e.SchedulePk)
                    .HasName("schedules_pkey");

                entity.ToTable("schedules");

                entity.Property(e => e.SchedulePk)
                    .HasColumnName("schedule_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("date");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedules_group_fk_fkey");
            });

            modelBuilder.Entity<Teams>(entity =>
            {
                entity.HasKey(e => e.TeamPk)
                    .HasName("teams_pkey");

                entity.ToTable("teams");

                entity.Property(e => e.TeamPk)
                    .HasColumnName("team_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.DepartmentFk).HasColumnName("department_fk");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.DepartmentFkNavigation)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.DepartmentFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("teams_department_fk_fkey");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("teams_group_fk_fkey");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserPk)
                    .HasName("users_pkey");

                entity.ToTable("users");

                entity.Property(e => e.UserPk)
                    .HasColumnName("user_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.AcceptTerms).HasColumnName("accept_terms");

                entity.Property(e => e.Activated).HasColumnName("activated");

                entity.Property(e => e.Administrator).HasColumnName("administrator");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasColumnName("email_address")
                    .HasMaxLength(256);

                entity.Property(e => e.GoogleLogon).HasColumnName("google_logon");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(512);

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnName("user_name")
                    .HasMaxLength(256);

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_group_fk_fkey");
            });
        }
    }
}
