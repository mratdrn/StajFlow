using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261114181005_AddPostingShortDescription")]
    partial class AddPostingShortDescription
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StajFlow.Api.Entities.StudentProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("FirstName");

                    b.Property<string>("Biography")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)")
                        .HasColumnName("Biography");

                    b.Property<string>("Department")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Department");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("nvarchar(254)")
                        .HasColumnName("Email");

                    b.Property<string>("IdentityUserId")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("IdentityUserId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("LastName");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasColumnName("PhoneNumber");

                    b.Property<string>("University")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("University");

                    b.HasKey("Id");

                    b.HasIndex("IdentityUserId")
                        .IsUnique()
                        .HasFilter("[IdentityUserId] IS NOT NULL");

                    b.ToTable("StudentProfiles");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.Resume", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("StorageKey")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("StorageKey");

                    b.Property<long>("FileSizeBytes")
                        .HasColumnType("bigint")
                        .HasColumnName("FileSizeBytes");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("ContentType");

                    b.Property<int>("StudentProfileId")
                        .HasColumnType("int")
                        .HasColumnName("StudentProfileId");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("OriginalFileName");

                    b.Property<DateTimeOffset>("UploadedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("UploadedAt")
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

            modelBuilder.Entity("StajFlow.Api.Entities.CompanyProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(6000)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Name");

                    b.Property<string>("ContactEmail")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("nvarchar(254)")
                        .HasColumnName("ContactEmail");

                    b.Property<string>("Location")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)")
                        .HasColumnName("Location");

                    b.Property<string>("IdentityUserId")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("IdentityUserId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Industry")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("Industry");

                    b.Property<string>("WebsiteUrl")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)")
                        .HasColumnName("WebsiteUrl");

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
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("AppliedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("AppliedAt")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset?>("StatusUpdatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("StatusUpdatedAt");

                    b.Property<int>("StudentProfileId")
                        .HasColumnType("int")
                        .HasColumnName("StudentProfileId");

                    b.Property<string>("CoverLetter")
                        .HasMaxLength(6000)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CoverLetter");

                    b.Property<int>("ResumeId")
                        .HasColumnType("int")
                        .HasColumnName("ResumeId");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion")
                        .HasColumnName("RowVersion");

                    b.Property<int>("InternshipPostingId")
                        .HasColumnType("int")
                        .HasColumnName("InternshipPostingId");

                    b.HasKey("Id");

                    b.HasIndex("StudentProfileId", "InternshipPostingId")
                        .IsUnique();

                    b.HasIndex("ResumeId", "StudentProfileId");

                    b.HasIndex("InternshipPostingId", "Status");

                    b.ToTable("Applications", null, t =>
                        {
                            t.HasCheckConstraint("CK_Applications_Status", "[Status] IN ('Submitted','UnderReview','Shortlisted','Accepted','Rejected','Withdrawn')");

                            t.HasCheckConstraint("CK_Applications_StatusUpdatedAt", "[StatusUpdatedAt] IS NULL OR [StatusUpdatedAt] >= [AppliedAt]");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipRequirement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)")
                        .HasColumnName("Description");

                    b.Property<int>("InternshipPostingId")
                        .HasColumnType("int")
                        .HasColumnName("InternshipPostingId");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("Type");

                    b.HasKey("Id");

                    b.HasIndex("InternshipPostingId");

                    b.ToTable("InternshipRequirements", null, t =>
                        {
                            t.HasCheckConstraint("CK_InternshipRequirements_Type", "[Type] IN ('Required','Preferred')");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipPosting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Description");

                    b.Property<DateOnly?>("StartDate")
                        .HasColumnType("date")
                        .HasColumnName("StartDate");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Title");

                    b.Property<DateOnly?>("EndDate")
                        .HasColumnType("date")
                        .HasColumnName("EndDate");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("Status");

                    b.Property<string>("ShortDescription")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)")
                        .HasColumnName("ShortDescription");

                    b.Property<string>("Location")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)")
                        .HasColumnName("Location");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion")
                        .HasColumnName("RowVersion");

                    b.Property<int>("CompanyProfileId")
                        .HasColumnType("int")
                        .HasColumnName("CompanyProfileId");

                    b.Property<DateTimeOffset?>("ApplicationDeadline")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("ApplicationDeadline");

                    b.Property<bool>("IsRemote")
                        .HasColumnType("bit")
                        .HasColumnName("IsRemote");

                    b.HasKey("Id");

                    b.HasIndex("CompanyProfileId");

                    b.HasIndex("Status", "ApplicationDeadline");

                    b.ToTable("InternshipPostings", null, t =>
                        {
                            t.HasCheckConstraint("CK_InternshipPostings_Dates", "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");

                            t.HasCheckConstraint("CK_InternshipPostings_Status", "[Status] IN ('Draft','Published','Closed')");
                        });
                });

            modelBuilder.Entity("StajFlow.Api.Entities.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .IsUnique()
                        .HasDatabaseName("EmailIndex")
                        .HasFilter("[NormalizedEmail] IS NOT NULL");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "role-student",
                            ConcurrencyStamp = "role-student-v1",
                            Name = "Student",
                            NormalizedName = "STUDENT"
                        },
                        new
                        {
                            Id = "role-company",
                            ConcurrencyStamp = "role-company-v1",
                            Name = "Company",
                            NormalizedName = "COMPANY"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("StajFlow.Api.Entities.StudentProfile", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.ApplicationUser", null)
                        .WithOne("StudentProfile")
                        .HasForeignKey("StajFlow.Api.Entities.StudentProfile", "IdentityUserId")
                        .OnDelete(DeleteBehavior.NoAction);
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
                    b.HasOne("StajFlow.Api.Entities.ApplicationUser", null)
                        .WithOne("CompanyProfile")
                        .HasForeignKey("StajFlow.Api.Entities.CompanyProfile", "IdentityUserId")
                        .OnDelete(DeleteBehavior.NoAction);
                });

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipApplication", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.StudentProfile", "StudentProfile")
                        .WithMany("Applications")
                        .HasForeignKey("StudentProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("StajFlow.Api.Entities.InternshipPosting", "InternshipPosting")
                        .WithMany("Applications")
                        .HasForeignKey("InternshipPostingId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("StajFlow.Api.Entities.Resume", "Resume")
                        .WithMany("Applications")
                        .HasForeignKey("ResumeId", "StudentProfileId")
                        .HasPrincipalKey("Id", "StudentProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("StudentProfile");

                    b.Navigation("Resume");

                    b.Navigation("InternshipPosting");
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

            modelBuilder.Entity("StajFlow.Api.Entities.InternshipPosting", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.CompanyProfile", "CompanyProfile")
                        .WithMany("InternshipPostings")
                        .HasForeignKey("CompanyProfileId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("CompanyProfile");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StajFlow.Api.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("StajFlow.Api.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("StajFlow.Api.Entities.StudentProfile", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("Resumes");
                });

            modelBuilder.Entity("StajFlow.Api.Entities.Resume", b =>
                {
                    b.Navigation("Applications");
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

            modelBuilder.Entity("StajFlow.Api.Entities.ApplicationUser", b =>
                {
                    b.Navigation("StudentProfile");

                    b.Navigation("CompanyProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
