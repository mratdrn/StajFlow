using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261110143305_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StajFlow.Api.Entities.CompanyProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContactEmail")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("nvarchar(254)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Description")
                        .HasMaxLength(6000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IdentityUserId")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Industry")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("Location")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("WebsiteUrl")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.HasKey("Id");

                    b.HasIndex("IdentityUserId")
                        .IsUnique()
                        .HasFilter("[IdentityUserId] IS NOT NULL");

                    b.ToTable("CompanyProfiles");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipApplication", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("AppliedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("CoverLetter")
                        .HasMaxLength(6000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("InternshipPostingId")
                        .HasColumnType("int");

                    b.Property<int>("ResumeId")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTimeOffset?>("StatusUpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("StudentProfileId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InternshipPostingId", "Status");

                    b.HasIndex("ResumeId", "StudentProfileId");

                    b.HasIndex("StudentProfileId", "InternshipPostingId")
                        .IsUnique();

                    b.ToTable("Applications", null, t =>
                        {
                            t.HasCheckConstraint("CK_Applications_Status", "[Status] IN ('Submitted','UnderReview','Shortlisted','Accepted','Rejected','Withdrawn')");

                            t.HasCheckConstraint("CK_Applications_StatusUpdatedAt", "[StatusUpdatedAt] IS NULL OR [StatusUpdatedAt] >= [AppliedAt]");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipPosting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("ApplicationDeadline")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("CompanyProfileId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("EndDate")
                        .HasColumnType("date");

                    b.Property<bool>("IsRemote")
                        .HasColumnType("bit");

                    b.Property<string>("Location")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateOnly?>("StartDate")
                        .HasColumnType("date");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.HasIndex("CompanyProfileId");

                    b.HasIndex("Status", "ApplicationDeadline");

                    b.ToTable("InternshipPostings", null, t =>
                        {
                            t.HasCheckConstraint("CK_InternshipPostings_Dates", "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");

                            t.HasCheckConstraint("CK_InternshipPostings_Status", "[Status] IN ('Draft','Published','Closed')");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipRequirement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("InternshipPostingId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("InternshipPostingId");

                    b.ToTable("InternshipRequirements", null, t =>
                        {
                            t.HasCheckConstraint("CK_InternshipRequirements_Type", "[Type] IN ('Required','Preferred')");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.Resume", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<long>("FileSizeBytes")
                        .HasColumnType("bigint");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("StorageKey")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("StudentProfileId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UploadedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.HasKey("Id");

                    b.HasIndex("StorageKey")
                        .IsUnique();

                    b.HasIndex("StudentProfileId");

                    b.ToTable("Resumes", null, t =>
                        {
                            t.HasCheckConstraint("CK_Resumes_FileSizeBytes", "[FileSizeBytes] > 0");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.StudentProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Biography")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Department")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("nvarchar(254)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("IdentityUserId")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("University")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.HasIndex("IdentityUserId")
                        .IsUnique()
                        .HasFilter("[IdentityUserId] IS NOT NULL");

                    b.ToTable("StudentProfiles");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipApplication", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.InternshipPosting", "InternshipPosting")
                        .WithMany("Applications")
                        .HasForeignKey("InternshipPostingId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("StajFlow.Api.Entities.StudentProfile", "StudentProfile")
                        .WithMany("Applications")
                        .HasForeignKey("StudentProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("StajFlow.Api.Entities.Resume", "Resume")
                        .WithMany("Applications")
                        .HasForeignKey("ResumeId", "StudentProfileId")
                        .HasPrincipalKey("Id", "StudentProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("InternshipPosting");

                    b.Navigation("Resume");

                    b.Navigation("StudentProfile");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipPosting", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.CompanyProfile", "CompanyProfile")
                        .WithMany("InternshipPostings")
                        .HasForeignKey("CompanyProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("CompanyProfile");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipRequirement", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.InternshipPosting", "InternshipPosting")
                        .WithMany("Requirements")
                        .HasForeignKey("InternshipPostingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InternshipPosting");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.Resume", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.StudentProfile", "StudentProfile")
                        .WithMany("Resumes")
                        .HasForeignKey("StudentProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("StudentProfile");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.CompanyProfile", b =>
                {
                    b.Navigation("InternshipPostings");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipPosting", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("Requirements");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.Resume", b =>
                {
                    b.Navigation("Applications");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.StudentProfile", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("Resumes");
                });
#pragma warning restore 612, 618
        }
    }
}
