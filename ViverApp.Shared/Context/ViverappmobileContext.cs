using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.Context;

public partial class ViverappmobileContext : DbContext
{
    public ViverappmobileContext()
    {
    }

    public ViverappmobileContext(DbContextOptions<ViverappmobileContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentType> AppointmentTypes { get; set; }

    public virtual DbSet<AvailabilityClinic> AvailabilityClinics { get; set; }

    public virtual DbSet<AvailabilityDoctor> AvailabilityDoctors { get; set; }

    public virtual DbSet<Clinic> Clinics { get; set; }

    public virtual DbSet<Config> Configs { get; set; }

    public virtual DbSet<DoctorProp> DoctorProps { get; set; }

    public virtual DbSet<EmailConfirmation> EmailConfirmations { get; set; }

    public virtual DbSet<EmailQueue> EmailQueues { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<PremiumPlan> PremiumPlans { get; set; }

    public virtual DbSet<PremiumUser> PremiumUsers { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleAttachment> ScheduleAttachments { get; set; }

    public virtual DbSet<SpecialtysDoctor> SpecialtysDoctors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Idappointment).HasName("PRIMARY");

            entity.ToTable("appointment");

            entity.HasIndex(e => e.Idappointmenttype, "fk_atendimento_tipoatendimento_idx");

            entity.HasIndex(e => e.Title, "title_UNIQUE").IsUnique();

            entity.Property(e => e.Idappointment).HasColumnName("idappointment");
            entity.Property(e => e.Averagetime)
                .HasColumnType("time")
                .HasColumnName("averagetime");
            entity.Property(e => e.Canonline).HasColumnName("canonline");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Idappointmenttype).HasColumnName("idappointmenttype");
            entity.Property(e => e.Ispopular).HasColumnName("ispopular");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");

            entity.HasOne(d => d.IdappointmenttypeNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.Idappointmenttype)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointment_appointmenttype");
        });

        modelBuilder.Entity<AppointmentType>(entity =>
        {
            entity.HasKey(e => e.Idappointmenttype).HasName("PRIMARY");

            entity.ToTable("appointment_type");

            entity.Property(e => e.Idappointmenttype).HasColumnName("idappointmenttype");
            entity.Property(e => e.Description)
                .HasMaxLength(45)
                .HasColumnName("description");
        });

        modelBuilder.Entity<AvailabilityClinic>(entity =>
        {
            entity.HasKey(e => e.Idavailabilityclinic).HasName("PRIMARY");

            entity.ToTable("availability_clinic");

            entity.HasIndex(e => e.Idclinic, "fk_availabilityclinic_clinic_idx");

            entity.Property(e => e.Idavailabilityclinic).HasColumnName("idavailabilityclinic");
            entity.Property(e => e.Daytype).HasColumnName("daytype");
            entity.Property(e => e.Endtime)
                .HasColumnType("time")
                .HasColumnName("endtime");
            entity.Property(e => e.Idclinic).HasColumnName("idclinic");
            entity.Property(e => e.Starttime)
                .HasColumnType("time")
                .HasColumnName("starttime");

            entity.HasOne(d => d.IdclinicNavigation).WithMany(p => p.AvailabilityClinics)
                .HasForeignKey(d => d.Idclinic)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_availabilityclinic_clinic");
        });

        modelBuilder.Entity<AvailabilityDoctor>(entity =>
        {
            entity.HasKey(e => e.Idavailabilitydoctor).HasName("PRIMARY");

            entity.ToTable("availability_doctor");

            entity.HasIndex(e => e.Iddoctor, "fk_availabilitydoctor_doctor_idx");

            entity.Property(e => e.Idavailabilitydoctor).HasColumnName("idavailabilitydoctor");
            entity.Property(e => e.Daytype).HasColumnName("daytype");
            entity.Property(e => e.Endtime)
                .HasColumnType("time")
                .HasColumnName("endtime");
            entity.Property(e => e.Iddoctor).HasColumnName("iddoctor");
            entity.Property(e => e.Isonline).HasColumnName("isonline");
            entity.Property(e => e.Starttime)
                .HasColumnType("time")
                .HasColumnName("starttime");

            entity.HasOne(d => d.IddoctorNavigation).WithMany(p => p.AvailabilityDoctors)
                .HasForeignKey(d => d.Iddoctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_availabilitydoctor_doctor");
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.Idclinic).HasName("PRIMARY");

            entity.ToTable("clinic");

            entity.Property(e => e.Idclinic).HasColumnName("idclinic");
            entity.Property(e => e.Adress)
                .HasMaxLength(45)
                .HasColumnName("adress");
            entity.Property(e => e.City)
                .HasMaxLength(45)
                .HasColumnName("city");
            entity.Property(e => e.Cnpj)
                .HasMaxLength(18)
                .HasColumnName("cnpj");
            entity.Property(e => e.Complement)
                .HasMaxLength(45)
                .HasColumnName("complement");
            entity.Property(e => e.Corporatereason)
                .HasMaxLength(90)
                .HasColumnName("corporatereason");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Fantasyname)
                .HasMaxLength(90)
                .HasColumnName("fantasyname");
            entity.Property(e => e.Fone)
                .HasMaxLength(15)
                .HasColumnName("fone");
            entity.Property(e => e.Neighborhood)
                .HasMaxLength(45)
                .HasColumnName("neighborhood");
            entity.Property(e => e.Number)
                .HasMaxLength(4)
                .HasColumnName("number");
            entity.Property(e => e.Postalcode)
                .HasMaxLength(9)
                .HasColumnName("postalcode");
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .HasColumnName("state");
        });

        modelBuilder.Entity<Config>(entity =>
        {
            entity.HasKey(e => e.Idconfig).HasName("PRIMARY");

            entity.ToTable("config");

            entity.Property(e => e.Idconfig).HasColumnName("idconfig");
            entity.Property(e => e.Canshow)
                .HasDefaultValueSql("'1'")
                .HasColumnName("canshow");
            entity.Property(e => e.Description)
                .HasMaxLength(512)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.Valueisbool).HasColumnName("valueisbool");
        });

        modelBuilder.Entity<DoctorProp>(entity =>
        {
            entity.HasKey(e => e.Iddoctorprops).HasName("PRIMARY");

            entity.ToTable("doctor_props");

            entity.HasIndex(e => e.Iddoctor, "fk_doctorprops_user_idx").IsUnique();

            entity.Property(e => e.Iddoctorprops).HasColumnName("iddoctorprops");
            entity.Property(e => e.Attendonline).HasColumnName("attendonline");
            entity.Property(e => e.Crm)
                .HasMaxLength(15)
                .HasColumnName("crm");
            entity.Property(e => e.Iddoctor).HasColumnName("iddoctor");
            entity.Property(e => e.Mainspecialty)
                .HasMaxLength(45)
                .HasColumnName("mainspecialty");
            entity.Property(e => e.Maxonlinedayconsultation).HasColumnName("maxonlinedayconsultation");
            entity.Property(e => e.Maxpresencialdayconsultation).HasColumnName("maxpresencialdayconsultation");
            entity.Property(e => e.Medicalexperience).HasColumnName("medicalexperience");
            entity.Property(e => e.Rating)
                .HasColumnType("float(2,2)")
                .HasColumnName("rating");
            entity.Property(e => e.Title)
                .HasMaxLength(4)
                .HasColumnName("title");

            entity.HasOne(d => d.IddoctorNavigation).WithOne(p => p.DoctorProp)
                .HasForeignKey<DoctorProp>(d => d.Iddoctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_doctorprops_user");
        });

        modelBuilder.Entity<EmailConfirmation>(entity =>
        {
            entity.HasKey(e => e.Idemailconfirmation).HasName("PRIMARY");

            entity.ToTable("email_confirmation");

            entity.HasIndex(e => e.Idemail, "fk_emailconfirmation_emailqueue_idx").IsUnique();

            entity.Property(e => e.Idemailconfirmation).HasColumnName("idemailconfirmation");
            entity.Property(e => e.Confirmationcode).HasColumnName("confirmationcode");
            entity.Property(e => e.Expiresat)
                .HasColumnType("datetime")
                .HasColumnName("expiresat");
            entity.Property(e => e.Idemail).HasColumnName("idemail");

            entity.HasOne(d => d.IdemailNavigation).WithOne(p => p.EmailConfirmation)
                .HasForeignKey<EmailConfirmation>(d => d.Idemail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_emailconfirmation_emailqueue");
        });

        modelBuilder.Entity<EmailQueue>(entity =>
        {
            entity.HasKey(e => e.Idemail).HasName("PRIMARY");

            entity.ToTable("email_queue");

            entity.Property(e => e.Idemail).HasColumnName("idemail");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdat");
            entity.Property(e => e.Receiver)
                .HasMaxLength(45)
                .HasColumnName("receiver");
            entity.Property(e => e.Sender)
                .HasMaxLength(45)
                .HasColumnName("sender");
            entity.Property(e => e.Severity)
                .HasDefaultValueSql("'3'")
                .HasColumnName("severity");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasColumnType("tinytext")
                .HasColumnName("subject");
            entity.Property(e => e.Tries).HasColumnName("tries");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Idholiday).HasName("PRIMARY");

            entity.ToTable("holiday");

            entity.Property(e => e.Idholiday).HasColumnName("idholiday");
            entity.Property(e => e.Canschedule).HasColumnName("canschedule");
            entity.Property(e => e.Holidaydate).HasColumnName("holidaydate");
            entity.Property(e => e.Holidayname)
                .HasMaxLength(45)
                .HasColumnName("holidayname");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Idnotification).HasName("PRIMARY");

            entity.ToTable("notification");

            entity.Property(e => e.Idnotification).HasColumnName("idnotification");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(512)
                .HasColumnName("description");
            entity.Property(e => e.Pushdescription)
                .HasMaxLength(512)
                .HasColumnName("pushdescription");
            entity.Property(e => e.Notificationtype)
                .HasDefaultValueSql("'1'")
                .HasColumnName("notificationtype");
            entity.Property(e => e.Read).HasColumnName("read");
            entity.Property(e => e.Sent).HasColumnName("sent");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.Targetid).HasColumnName("targetid");
            entity.Property(e => e.Title)
                .HasMaxLength(512)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Idpayment).HasName("PRIMARY");

            entity.ToTable("payment");

            entity.HasIndex(e => e.Idpaymenttype, "fk_payment_paymenttype_idx");

            entity.HasIndex(e => e.Idschedule, "fk_payment_schedule_idx");

            entity.Property(e => e.Idpayment).HasColumnName("idpayment");
            entity.Property(e => e.Cardauthorization)
                .HasMaxLength(512)
                .HasColumnName("cardauthorization");
            entity.Property(e => e.Cardlast4)
                .HasMaxLength(4)
                .HasColumnName("cardlast4");
            entity.Property(e => e.Idpaymenttype).HasColumnName("idpaymenttype");
            entity.Property(e => e.Idschedule).HasColumnName("idschedule");
            entity.Property(e => e.Paidday)
                .HasColumnType("datetime")
                .HasColumnName("paidday");
            entity.Property(e => e.Paidonline).HasColumnName("paidonline");
            entity.Property(e => e.Paidprice)
                .HasPrecision(12, 2)
                .HasColumnName("paidprice");

            entity.HasOne(d => d.IdpaymenttypeNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Idpaymenttype)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_paymenttype");

            entity.HasOne(d => d.IdscheduleNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Idschedule)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_schedule");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Idpaymenttype).HasName("PRIMARY");

            entity.ToTable("payment_type");

            entity.Property(e => e.Idpaymenttype).HasColumnName("idpaymenttype");
            entity.Property(e => e.Description)
                .HasMaxLength(45)
                .HasColumnName("description");
        });

        modelBuilder.Entity<PremiumPlan>(entity =>
        {
            entity.HasKey(e => e.Idpremiumplan).HasName("PRIMARY");

            entity.ToTable("premium_plan");

            entity.Property(e => e.Idpremiumplan).HasColumnName("idpremiumplan");
            entity.Property(e => e.Plantype).HasColumnName("plantype");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.Testperiod)
                .HasMaxLength(2)
                .HasColumnName("testperiod");
        });

        modelBuilder.Entity<PremiumUser>(entity =>
        {
            entity.HasKey(e => e.IdpremiumUser).HasName("PRIMARY");

            entity.ToTable("premium_user");

            entity.HasIndex(e => e.Idpremiumplan, "fk_premiumuser_premiumplan_idx");

            entity.HasIndex(e => e.Iduser, "fk_premiumuser_user_idx");

            entity.Property(e => e.IdpremiumUser).HasColumnName("idpremium_user");
            entity.Property(e => e.Idpremiumplan).HasColumnName("idpremiumplan");
            entity.Property(e => e.Iduser).HasColumnName("iduser");
            entity.Property(e => e.Intestperiod).HasColumnName("intestperiod");
            entity.Property(e => e.Premiumdate)
                .HasColumnType("datetime")
                .HasColumnName("premiumdate");

            entity.HasOne(d => d.IdpremiumplanNavigation).WithMany(p => p.PremiumUsers)
                .HasForeignKey(d => d.Idpremiumplan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_premiumuser_premiumplan");

            entity.HasOne(d => d.IduserNavigation).WithMany(p => p.PremiumUsers)
                .HasForeignKey(d => d.Iduser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_premiumuser_user");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Idschedule).HasName("PRIMARY");

            entity.ToTable("schedule");

            entity.HasIndex(e => e.Idclinic, "fk_appointment_clinic_idx");

            entity.HasIndex(e => e.Idappointment, "fk_appointmentuser_appointment_idx");

            entity.HasIndex(e => e.Iddoctor, "fk_appointmentuser_doctor_idx");

            entity.HasIndex(e => e.Iduser, "fk_appointmentuser_user_idx");

            entity.Property(e => e.Idschedule).HasColumnName("idschedule");
            entity.Property(e => e.Appointmentdate)
                .HasColumnType("datetime")
                .HasColumnName("appointmentdate");
            entity.Property(e => e.Feedback)
                .HasColumnType("text")
                .HasColumnName("feedback");
            entity.Property(e => e.Idappointment).HasColumnName("idappointment");
            entity.Property(e => e.Idclinic).HasColumnName("idclinic");
            entity.Property(e => e.Iddoctor).HasColumnName("iddoctor");
            entity.Property(e => e.Iduser).HasColumnName("iduser");
            entity.Property(e => e.Isonline).HasColumnName("isonline");
            entity.Property(e => e.Callconcluded).HasColumnName("callconcluded");
            entity.Property(e => e.Medicalreport)
                .HasColumnType("text")
                .HasColumnName("medicalreport");
            entity.Property(e => e.Obs)
                .HasColumnType("text")
                .HasColumnName("obs");
            entity.Property(e => e.Originaldate)
                .HasColumnType("datetime")
                .HasColumnName("originaldate");
            entity.Property(e => e.Pendingpayment)
                .HasDefaultValueSql("'1'")
                .HasColumnName("pendingpayment");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Rescheduled).HasColumnName("rescheduled");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdat");

            entity.HasOne(d => d.IdappointmentNavigation).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.Idappointment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointmentuser_appointment");

            entity.HasOne(d => d.IdclinicNavigation).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.Idclinic)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointment_clinic");

            entity.HasOne(d => d.IddoctorNavigation).WithMany(p => p.ScheduleIddoctorNavigations)
                .HasForeignKey(d => d.Iddoctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointmentuser_doctor");

            entity.HasOne(d => d.IduserNavigation).WithMany(p => p.ScheduleIduserNavigations)
                .HasForeignKey(d => d.Iduser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointmentuser_user");
        });

        modelBuilder.Entity<ScheduleAttachment>(entity =>
        {
            entity.HasKey(e => e.Idscheduleattachments).HasName("PRIMARY");

            entity.ToTable("schedule_attachments");

            entity.HasIndex(e => e.Idschedule, "fk_scheduleattachments_schedule_idx");

            entity.Property(e => e.Idscheduleattachments).HasColumnName("idscheduleattachments");
            entity.Property(e => e.Filepath)
                .HasMaxLength(512)
                .HasColumnName("filepath");
            entity.Property(e => e.Filename)
               .HasMaxLength(512)
               .HasColumnName("filename");
            entity.Property(e => e.Size)
                .HasColumnType("float")
                .HasColumnName("size");
            entity.Property(e => e.Idschedule).HasColumnName("idschedule");

            entity.HasOne(d => d.IdscheduleNavigation).WithMany(p => p.ScheduleAttachments)
                .HasForeignKey(d => d.Idschedule)
                .HasConstraintName("fk_scheduleattachments_schedule");
        });

        modelBuilder.Entity<SpecialtysDoctor>(entity =>
        {
            entity.HasKey(e => e.Idspecialtysdoctor).HasName("PRIMARY");

            entity.ToTable("specialtys_doctor");

            entity.HasIndex(e => e.Idappointment, "fk_specialtysdoctor_appointment_idx");

            entity.HasIndex(e => e.Iddoctor, "fk_specialtysdoctor_doctor_idx");

            entity.Property(e => e.Idspecialtysdoctor).HasColumnName("idspecialtysdoctor");
            entity.Property(e => e.Idappointment).HasColumnName("idappointment");
            entity.Property(e => e.Iddoctor).HasColumnName("iddoctor");

            entity.HasOne(d => d.IdappointmentNavigation).WithMany(p => p.SpecialtysDoctors)
                .HasForeignKey(d => d.Idappointment)
                .HasConstraintName("fk_specialtysdoctor_appointment");

            entity.HasOne(d => d.IddoctorNavigation).WithMany(p => p.SpecialtysDoctors)
                .HasForeignKey(d => d.Iddoctor)
                .HasConstraintName("fk_specialtysdoctor_doctor");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Iduser).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.Iduser).HasColumnName("iduser");
            entity.Property(e => e.Adress)
                .HasMaxLength(45)
                .HasColumnName("adress");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.City)
                .HasMaxLength(45)
                .HasColumnName("city");
            entity.Property(e => e.Complement)
                .HasMaxLength(45)
                .HasColumnName("complement");
            entity.Property(e => e.Cpf)
                .HasMaxLength(14)
                .HasColumnName("cpf");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdat");
            entity.Property(e => e.Devicetoken)
                .HasColumnName("devicetoken")
                .HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Fone)
                .HasMaxLength(15)
                .HasColumnName("fone");
            entity.Property(e => e.Ispremium).HasColumnName("ispremium");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Neighborhood)
                .HasMaxLength(45)
                .HasColumnName("neighborhood");
            entity.Property(e => e.Notifyemail).HasColumnName("notifyemail");
            entity.Property(e => e.Notifypush).HasColumnName("notifypush");
            entity.Property(e => e.Number)
                .HasMaxLength(45)
                .HasColumnName("number");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.Postalcode)
                .HasMaxLength(9)
                .HasColumnName("postalcode");
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .HasColumnName("state");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Usertype).HasColumnName("usertype");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Idusertoken).HasName("PRIMARY");

            entity.ToTable("user_token");

            entity.HasIndex(e => e.Iduser, "fk_usertokens_user_idx");

            entity.Property(e => e.Idusertoken).HasColumnName("idusertoken");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.Iduser).HasColumnName("iduser");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.Token)
                .HasMaxLength(512)
                .HasColumnName("token");

            entity.HasOne(d => d.IduserNavigation).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.Iduser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usertokens_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
