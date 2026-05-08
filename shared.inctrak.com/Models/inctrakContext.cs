using System;
using IncTrak.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IncTrak.Models
{
    public partial class inctrakContext : DbContext
    {
        public inctrakContext()
        {
        }

        public inctrakContext(DbContextOptions<inctrakContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AmountTypes> AmountTypes { get; set; }
        public virtual DbSet<Grants> Grants { get; set; }
        public object GRANTS { get; internal set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<ParticipantTypes> ParticipantTypes { get; set; }
        public virtual DbSet<Participants> Participants { get; set; }
        public virtual DbSet<PeriodTypes> PeriodTypes { get; set; }
        public virtual DbSet<Periods> Periods { get; set; }
        public virtual DbSet<Plans> Plans { get; set; }
        public virtual DbSet<Schedules> Schedules { get; set; }
        public virtual DbSet<StockClasses> StockClasses { get; set; }
        public virtual DbSet<StockHolders> StockHolders { get; set; }
        public virtual DbSet<TermFroms> TermFroms { get; set; }
        public virtual DbSet<Terminations> Terminations { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp")
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<AmountTypes>(entity =>
            {
                entity.HasKey(e => e.AmountTypePk)
                    .HasName("amount_types_pkey");

                entity.ToTable("amount_types");

                entity.Property(e => e.AmountTypePk)
                    .HasColumnName("amount_type_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Grants>(entity =>
            {
                entity.HasKey(e => e.GrantPk)
                    .HasName("grants_pkey");

                entity.ToTable("grants");

                entity.Property(e => e.GrantPk)
                    .HasColumnName("grant_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.DateOfGrant)
                    .HasColumnName("date_of_grant")
                    .HasColumnType("date");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.OptionPrice)
                    .HasColumnName("option_price")
                    .HasColumnType("numeric(18,6)");

                entity.Property(e => e.ParticipantFk).HasColumnName("participant_fk");

                entity.Property(e => e.PlanFk).HasColumnName("plan_fk");

                entity.Property(e => e.Shares)
                    .HasColumnName("shares")
                    .HasColumnType("numeric(18,0)");

                entity.Property(e => e.TerminationFk).HasColumnName("termination_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.VestingScheduleFk).HasColumnName("vesting_schedule_fk");

                entity.Property(e => e.VestingStart)
                    .HasColumnName("vesting_start")
                    .HasColumnType("date");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Grants)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("grants_group_fk_fkey");

                entity.HasOne(d => d.ParticipantFkNavigation)
                    .WithMany(p => p.Grants)
                    .HasForeignKey(d => d.ParticipantFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("grants_participant_fk_fkey");

                entity.HasOne(d => d.PlanFkNavigation)
                    .WithMany(p => p.Grants)
                    .HasForeignKey(d => d.PlanFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("grants_plan_fk_fkey");

                entity.HasOne(d => d.TerminationFkNavigation)
                    .WithMany(p => p.Grants)
                    .HasForeignKey(d => d.TerminationFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("grants_termination_fk_fkey");

                entity.HasOne(d => d.VestingScheduleFkNavigation)
                    .WithMany(p => p.Grants)
                    .HasForeignKey(d => d.VestingScheduleFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("grants_vesting_schedule_fk_fkey");
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

            modelBuilder.Entity<ParticipantTypes>(entity =>
            {
                entity.HasKey(e => e.ParticipantTypePk)
                    .HasName("participant_types_pkey");

                entity.ToTable("participant_types");

                entity.Property(e => e.ParticipantTypePk)
                    .HasColumnName("participant_type_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Participants>(entity =>
            {
                entity.HasKey(e => e.ParticipantPk)
                    .HasName("participants_pkey");

                entity.ToTable("participants");

                entity.Property(e => e.ParticipantPk)
                    .HasColumnName("participant_pk")
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

                entity.Property(e => e.ParticipantTypeFk).HasColumnName("participant_type_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.UserFk).HasColumnName("user_fk");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Participants)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("participants_group_fk_fkey");

                entity.HasOne(d => d.ParticipantTypeFkNavigation)
                    .WithMany(p => p.Participants)
                    .HasForeignKey(d => d.ParticipantTypeFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("participants_participant_type_fk_fkey");

                entity.HasOne(d => d.UserFkNavigation)
                    .WithMany(p => p.Participants)
                    .HasForeignKey(d => d.UserFk)
                    .HasConstraintName("participants_user_fk_fkey");
            });

            modelBuilder.Entity<PeriodTypes>(entity =>
            {
                entity.HasKey(e => e.PeriodTypePk)
                    .HasName("period_types_pkey");

                entity.ToTable("period_types");

                entity.Property(e => e.PeriodTypePk)
                    .HasColumnName("period_type_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Periods>(entity =>
            {
                entity.HasKey(e => e.PeriodPk)
                    .HasName("periods_pkey");

                entity.ToTable("periods");

                entity.Property(e => e.PeriodPk)
                    .HasColumnName("period_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("numeric(18,6)");

                entity.Property(e => e.AmountTypeFk).HasColumnName("amount_type_fk");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.EvenOverN).HasColumnName("even_over_n");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.Increments).HasColumnName("increments");

                entity.Property(e => e.Order).HasColumnName("ORDER");

                entity.Property(e => e.PeriodAmount).HasColumnName("period_amount");

                entity.Property(e => e.PeriodTypeFk).HasColumnName("period_type_fk");

                entity.Property(e => e.ScheduleFk).HasColumnName("schedule_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.AmountTypeFkNavigation)
                    .WithMany(p => p.Periods)
                    .HasForeignKey(d => d.AmountTypeFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("periods_amount_type_fk_fkey");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Periods)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("periods_group_fk_fkey");

                entity.HasOne(d => d.PeriodTypeFkNavigation)
                    .WithMany(p => p.Periods)
                    .HasForeignKey(d => d.PeriodTypeFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("periods_period_type_fk_fkey");

                entity.HasOne(d => d.ScheduleFkNavigation)
                    .WithMany(p => p.Periods)
                    .HasForeignKey(d => d.ScheduleFk)
                    .HasConstraintName("periods_schedule_fk_fkey");
            });

            modelBuilder.Entity<Plans>(entity =>
            {
                entity.HasKey(e => e.PlanPk)
                    .HasName("plans_pkey");

                entity.ToTable("plans");

                entity.Property(e => e.PlanPk)
                    .HasColumnName("plan_pk")
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

                entity.Property(e => e.StockClassFk).HasColumnName("stock_class_fk");

                entity.Property(e => e.TotalShares)
                    .HasColumnName("total_shares")
                    .HasColumnType("numeric(18,0)");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Plans)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plans_group_fk_fkey");

                entity.HasOne(d => d.StockClassFkNavigation)
                    .WithMany(p => p.Plans)
                    .HasForeignKey(d => d.StockClassFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plans_stock_class_fk_fkey");
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
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedules_group_fk_fkey");
            });

            modelBuilder.Entity<StockClasses>(entity =>
            {
                entity.HasKey(e => e.StockClassPk)
                    .HasName("stock_classes_pkey");

                entity.ToTable("stock_classes");

                entity.Property(e => e.StockClassPk)
                    .HasColumnName("stock_class_pk")
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

                entity.Property(e => e.TotalShares)
                    .HasColumnName("total_shares")
                    .HasColumnType("numeric(18,0)");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.StockClasses)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("stock_classes_group_fk_fkey");
            });

            modelBuilder.Entity<StockHolders>(entity =>
            {
                entity.HasKey(e => e.StockHolderPk)
                    .HasName("stock_holders_pkey");

                entity.ToTable("stock_holders");

                entity.Property(e => e.StockHolderPk)
                    .HasColumnName("stock_holder_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.DateOfSale)
                    .HasColumnName("date_of_sale")
                    .HasColumnType("date");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.ParticipantFk).HasColumnName("participant_fk");

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("numeric(18,6)");

                entity.Property(e => e.Shares)
                    .HasColumnName("shares")
                    .HasColumnType("numeric(18,0)");

                entity.Property(e => e.StockClassFk).HasColumnName("stock_class_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.StockHolders)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("stock_holders_group_fk_fkey");

                entity.HasOne(d => d.ParticipantFkNavigation)
                    .WithMany(p => p.StockHolders)
                    .HasForeignKey(d => d.ParticipantFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("stock_holders_participant_fk_fkey");

                entity.HasOne(d => d.StockClassFkNavigation)
                    .WithMany(p => p.StockHolders)
                    .HasForeignKey(d => d.StockClassFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("stock_holders_stock_class_fk_fkey");
            });

            modelBuilder.Entity<TermFroms>(entity =>
            {
                entity.HasKey(e => e.TermFromPk)
                    .HasName("term_froms_pkey");

                entity.ToTable("term_froms");

                entity.Property(e => e.TermFromPk)
                    .HasColumnName("term_from_pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Terminations>(entity =>
            {
                entity.HasKey(e => e.TerminationPk)
                    .HasName("terminations_pkey");

                entity.ToTable("terminations");

                entity.Property(e => e.TerminationPk)
                    .HasColumnName("termination_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.AbsoluteDate)
                    .HasColumnName("absolute_date")
                    .HasColumnType("date");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Days).HasColumnName("days");

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

                entity.Property(e => e.IsAbsolute).HasColumnName("is_absolute");

                entity.Property(e => e.Months).HasColumnName("months");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.SpecificDate)
                    .HasColumnName("specific_date")
                    .HasColumnType("date");

                entity.Property(e => e.TermFromFk).HasColumnName("term_from_fk");

                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Years).HasColumnName("years");

                entity.HasOne(d => d.GroupFkNavigation)
                    .WithMany(p => p.Terminations)
                    .HasForeignKey(d => d.GroupFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("terminations_group_fk_fkey");

                entity.HasOne(d => d.TermFromFkNavigation)
                    .WithMany(p => p.Terminations)
                    .HasForeignKey(d => d.TermFromFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("terminations_term_from_fk_fkey");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserPk)
                    .HasName("users_pkey");

                entity.ToTable("users");

                entity.Property(e => e.UserPk)
                    .HasColumnName("user_pk")
                    .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Administrator).HasColumnName("administrator");

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasColumnName("email_address")
                    .HasMaxLength(256);

                entity.Property(e => e.GroupFk).HasColumnName("group_fk");

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
