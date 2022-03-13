using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WIPPS_API_3._0.Models.Procedure;

namespace WIPPS_API_3._0.Models
{
    public partial class SafetymanContext : DbContext
    {
        public SafetymanContext()
        {
        }

        public SafetymanContext(DbContextOptions<SafetymanContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Config> Configs { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<AreaEmail> AreaEmails { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<ComponentAttachment> ComponentAttachments { get; set; }
        public virtual DbSet<ComponentChecklist> ComponentChecklists { get; set; }
        public virtual DbSet<ComponentChecklist2> ComponentChecklists2 { get; set; }
        public virtual DbSet<Form> Forms { get; set; }
        public virtual DbSet<Form2> Forms2 { get; set; }
        public virtual DbSet<FormPercentage> FormPercentages { get; set; }
        public virtual DbSet<FormComponent> FormComponents { get; set; }
        public virtual DbSet<FormComponent2> FormComponents2 { get; set; }
        public virtual DbSet<FormType> FormTypes { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemInspection> ItemInspections { get; set; }
        public virtual DbSet<ItemInspection2> ItemInspections2 { get; set; }
        public virtual DbSet<ItemInspectionChecklist> ItemInspectionChecklists { get; set; }
        public virtual DbSet<ItemInspectionChecklistAttachment> ItemInspectionChecklistAttachments { get; set; }
        public virtual DbSet<ItemRequirement> ItemRequirements { get; set; }
        public virtual DbSet<ItemRequirement2> ItemRequirements2 { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<ModelHasPermission> ModelHasPermissions { get; set; }
        public virtual DbSet<ModelHasRole> ModelHasRoles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Refinery> Refineries { get; set; }
        public virtual DbSet<Requirement> Requirements { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleHasPermission> RoleHasPermissions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Weather> Weathers { get; set; }

        public virtual DbSet<GetAreaSafe> GetAreaSafe { get; set; }
        public virtual DbSet<GetInspection> GetInspections { get; set; }
        public virtual DbSet<GetTypeSafe> GetTypeSafe { get; set; }
        public virtual DbSet<GetLocation> GetLocations { get; set; }
        public virtual DbSet<GetCompany> GetCompanies { get; set; }
        public virtual DbSet<GetInspectorInspection> GetInspectorInspections { get; set; }
        public virtual DbSet<GetCompanyInspection> GetCompanyInspections { get; set; }
        public virtual DbSet<GetInspectorScore> GetInspectorScores { get; set; }
        public virtual DbSet<GetSvsu> GetSvsu { get; set; }
        public virtual DbSet<FormType2> FormTypes2 { get; set; }
        public virtual DbSet<FormType3> FormTypes3 { get; set; }

        public virtual DbSet<Chart.GetAreaSafe> ChartGetAreaSafe { get; set; }
        public virtual DbSet<Chart.GetCompany> ChartGetCompanies { get; set; }
        public virtual DbSet<Chart.GetCompanyInspection> ChartGetCompanyInspections { get; set; }
        public virtual DbSet<Chart.GetInspection> ChartGetInspections { get; set; }
        public virtual DbSet<Chart.GetInspectorInspection> ChartGetInspectorInspections { get; set; }
        public virtual DbSet<Chart.GetInspectorScore> ChartGetInspectorScores { get; set; }
        public virtual DbSet<Chart.GetLocation> ChartGetLocations { get; set; }
        public virtual DbSet<Chart.GetSvsu> ChartGetSvsu { get; set; }
        public virtual DbSet<Chart.GetTypeSafe> ChartGetTypeSafe { get; set; }
        public virtual DbSet<Chart.GetTypeUnsafe> ChartGetTypeUnsafe { get; set; }
        public virtual DbSet<Chart.GetDetailType> ChartGetDetailType { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=DESKTOP-9H7BFPI;Database=Safetyman;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GetSvsu>(entity =>
            {

                entity.Property(e => e.safe)
                    .HasColumnName("safe")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.@unsafe)
                    .HasColumnName("unsafe")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetAreaSafe>(entity =>
            {
                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.pos)
                    .HasColumnName("pos")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.neg)
                    .HasColumnName("neg")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetInspection>(entity =>
            {
                entity.Property(e => e.inspection)
                    .HasColumnName("inspection");

                entity.Property(e => e.pos)
                    .HasColumnName("pos");

                entity.Property(e => e.neg)
                    .HasColumnName("neg");

                entity.Property(e => e.safe)
                    .HasColumnName("safe")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.@unsafe)
                    .HasColumnName("unsafe")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.percentage)
                    .HasColumnName("percentage")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.month)
                    .HasColumnName("month");

                entity.Property(e => e.month_n)
                    .HasColumnName("month_n");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetTypeSafe>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.file)
                    .HasColumnName("file");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.ord)
                    .HasColumnName("ord");

                entity.Property(e => e.elipsis)
                    .HasColumnName("elipsis");

                entity.Property(e => e.checklists_count)
                    .HasColumnName("checklists_count");

                entity.Property(e => e.checklists_pos_count)
                    .HasColumnName("checklists_pos_count");

                entity.Property(e => e.checklists_neg_count)
                    .HasColumnName("checklists_neg_count");

                entity.Property(e => e.pos)
                    .HasColumnName("pos")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.neg)
                    .HasColumnName("neg")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.percentage)
                    .HasColumnName("percentage")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.percentagen)
                    .HasColumnName("percentagen")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.total)
                    .HasColumnName("totals")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetLocation>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.forms_count)
                    .HasColumnName("forms_count");

                entity.Property(e => e.total)
                    .HasColumnName("total");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetCompany>(entity =>
            {
                entity.Property(e => e.companies)
                    .HasColumnName("companies");

                entity.Property(e => e.company)
                    .HasColumnName("company");

                entity.Property(e => e.pos)
                    .HasColumnName("pos");

                entity.Property(e => e.neg)
                    .HasColumnName("neg");

                entity.Property(e => e.safe)
                    .HasColumnName("safe")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.@unsafe)
                    .HasColumnName("unsafe")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetInspectorInspection>(entity =>
            {
                entity.Property(e => e.user)
                    .HasColumnName("NameUser");

                entity.Property(e => e.user_slug)
                    .HasColumnName("user_slug");

                entity.Property(e => e.total)
                    .HasColumnName("total");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetCompanyInspection>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.type)
                    .HasColumnName("type");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.forms_count)
                    .HasColumnName("forms_count");

                entity.Property(e => e.total)
                    .HasColumnName("total");

                entity.HasNoKey();
            });

            modelBuilder.Entity<GetInspectorScore>(entity =>
            {
                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.pos)
                    .HasColumnName("pos")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.neg)
                    .HasColumnName("neg")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<FormType2>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.file)
                    .HasColumnName("file");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.ord)
                    .HasColumnName("ord");

                entity.Property(e => e.elipsis)
                    .HasColumnName("elipsis");

                entity.Property(e => e.checklists_count)
                    .HasColumnName("checklists_count")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.checklists_pos_count)
                    .HasColumnName("checklists_pos_count")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.checklists_neg_count)
                    .HasColumnName("checklists_neg_count")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.pos)
                    .HasColumnName("pos")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.neg)
                    .HasColumnName("neg")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.percentage)
                    .HasColumnName("percentage")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.percentagen)
                    .HasColumnName("percentagen")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.total)
                    .HasColumnName("totals")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<FormType3>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.file)
                    .HasColumnName("file");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.ord)
                    .HasColumnName("ord");

                entity.Property(e => e.checklists_count)
                    .HasColumnName("checklists_count");

                entity.Property(e => e.checklists_pos_count)
                    .HasColumnName("checklists_pos_count");

                entity.Property(e => e.checklists_neg_count)
                    .HasColumnName("checklists_neg_count");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetAreaSafe>(entity =>
            {
                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.pos)
                    .HasColumnName("pos");

                entity.Property(e => e.neg)
                    .HasColumnName("neg");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetCompany>(entity =>
            {
                entity.Property(e => e.companies)
                    .HasColumnName("companies");

                entity.Property(e => e.company)
                    .HasColumnName("company");

                entity.Property(e => e.pos)
                    .HasColumnName("pos");

                entity.Property(e => e.neg)
                    .HasColumnName("neg");

                entity.Property(e => e.safe)
                    .HasColumnName("safe");

                entity.Property(e => e.@unsafe)
                    .HasColumnName("unsafe");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetCompanyInspection>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.type)
                    .HasColumnName("type");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.forms_count)
                    .HasColumnName("forms_count");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetInspection>(entity =>
            {
                entity.Property(e => e.inspection)
                    .HasColumnName("inspection");

                entity.Property(e => e.safe)
                    .HasColumnName("safe");

                entity.Property(e => e.@unsafe)
                    .HasColumnName("unsafe");

                entity.Property(e => e.month)
                    .HasColumnName("month");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetInspectorInspection>(entity =>
            {
                entity.Property(e => e.user)
                    .HasColumnName("username");

                entity.Property(e => e.total)
                    .HasColumnName("total");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetInspectorScore>(entity =>
            {
                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.pos)
                    .HasColumnName("pos");

                entity.Property(e => e.neg)
                    .HasColumnName("neg");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetLocation>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.forms_count)
                    .HasColumnName("forms_count");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetSvsu>(entity =>
            {

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.status)
                    .HasColumnName("status");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetTypeSafe>(entity =>
            {
                entity.Property(e => e.id)
                    .HasColumnName("id");

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.file)
                    .HasColumnName("file");

                entity.Property(e => e.refinery_id)
                    .HasColumnName("refinery_id");

                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");

                entity.Property(e => e.updated_at)
                    .HasColumnName("updated_at");

                entity.Property(e => e.ord)
                    .HasColumnName("ord");

                entity.Property(e => e.checklists_count)
                    .HasColumnName("checklists_count");

                entity.Property(e => e.checklists_pos_count)
                    .HasColumnName("checklists_pos_count");

                entity.Property(e => e.checklists_neg_count)
                    .HasColumnName("checklists_neg_count");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetTypeUnsafe>(entity =>
            {

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.pos)
                    .HasColumnName("pos");

                entity.Property(e => e.neg)
                    .HasColumnName("neg");

                entity.Property(e => e.total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(5, 2)"); 

                entity.HasNoKey();
            });

            modelBuilder.Entity<Chart.GetDetailType>(entity =>
            {

                entity.Property(e => e.name)
                    .HasColumnName("name");

                entity.Property(e => e.slug)
                    .HasColumnName("slug");

                entity.Property(e => e.type)
                    .HasColumnName("type");

                entity.Property(e => e.form_component_id)
                    .HasColumnName("form_component_id");

                entity.Property(e => e.status)
                    .HasColumnName("status");

                entity.Property(e => e.total)
                    .HasColumnName("total");

                entity.HasNoKey();
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.ToTable("config");

                entity.Property(e => e.EmailTemplate)
                    .IsRequired()
                    .HasColumnName("email_template")
                    .HasMaxLength(int.MaxValue);

                entity.HasNoKey();
            });

            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("areas");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("areas_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("areas_ibfk_1");
            });

            modelBuilder.Entity<AreaEmail>(entity =>
            {
                entity.ToTable("area_emails");

                entity.HasIndex(e => e.AreaId)
                    .HasName("area_emails_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AreaId).HasColumnName("area_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.AreaEmails)
                    .HasForeignKey(d => d.AreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("area_emails_ibfk_1");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("companies_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("companies_ibfk_1");
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.ToTable("components");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("components_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.Components)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("components_ibfk_1");
            });

            modelBuilder.Entity<ComponentAttachment>(entity =>
            {
                entity.ToTable("component_attachments");

                entity.HasIndex(e => e.ComponentChecklistId)
                    .HasName("component_attachments_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ComponentChecklistId).HasColumnName("component_checklist_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.File)
                    .IsRequired()
                    .HasColumnName("file")
                    .HasColumnType("text");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.ComponentChecklist)
                    .WithMany(p => p.ComponentAttachments)
                    .HasForeignKey(d => d.ComponentChecklistId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("component_attachments_ibfk_1");
            });

            modelBuilder.Entity<ComponentChecklist>(entity =>
            {
                entity.ToTable("component_checklists");

                entity.HasIndex(e => e.FormComponentId)
                    .HasName("component_checklists_ibfk_2");

                entity.HasIndex(e => e.FormId)
                    .HasName("component_checklists_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.FormComponentId).HasColumnName("form_component_id");

                entity.Property(e => e.FormId).HasColumnName("form_id");

                entity.Property(e => e.SafeValue).HasColumnName("safe_value");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UnsafeValue).HasColumnName("unsafe_value");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.FormComponent)
                    .WithMany(p => p.ComponentChecklists)
                    .HasForeignKey(d => d.FormComponentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("component_checklists_ibfk_2");

                entity.HasOne(d => d.Form)
                    .WithMany(p => p.ComponentChecklists)
                    .HasForeignKey(d => d.FormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("component_checklists_ibfk_1");
            });

            modelBuilder.Entity<ComponentChecklist2>(entity =>
            {

                entity.HasIndex(e => e.FormComponentId)
                    .HasName("component_checklists_ibfk_2");

                entity.HasIndex(e => e.FormId)
                    .HasName("component_checklists_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.FormComponentId).HasColumnName("form_component_id");

                entity.Property(e => e.FormId).HasColumnName("form_id");

                entity.Property(e => e.SafeValue).HasColumnName("safe_value");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UnsafeValue).HasColumnName("unsafe_value");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .HasColumnName("Type");

                entity.Property(e => e.Slug)
                    .HasColumnName("Slug");

                entity.Property(e => e.Name)
                    .HasColumnName("Name");

                entity.Property(e => e.Total)
                    .HasColumnName("Total");
            });

            modelBuilder.Entity<Form>(entity =>
            {
                entity.ToTable("forms");

                entity.HasIndex(e => e.AreaId)
                    .HasName("forms_ibfk_1");

                entity.HasIndex(e => e.CompanyId)
                    .HasName("forms_ibfk_2");

                entity.HasIndex(e => e.FormTypeId)
                    .HasName("forms_ibfk_3");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AreaId).HasColumnName("area_id");

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");

                entity.Property(e => e.Equipment)
                    .HasColumnName("equipment")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FormTypeId).HasColumnName("form_type_id");

                entity.Property(e => e.JobId).HasColumnName("job_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.Forms)
                    .HasForeignKey(d => d.AreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("forms_ibfk_1");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Forms)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("forms_ibfk_2");

                entity.HasOne(d => d.FormType)
                    .WithMany(p => p.Forms)
                    .HasForeignKey(d => d.FormTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("forms_ibfk_3");
            });

            modelBuilder.Entity<Form2>(entity =>
            {

                entity.HasIndex(e => e.AreaId)
                    .HasName("forms_ibfk_1");

                entity.HasIndex(e => e.CompanyId)
                    .HasName("forms_ibfk_2");

                entity.HasIndex(e => e.FormTypeId)
                    .HasName("forms_ibfk_3");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AreaId).HasColumnName("area_id");

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");

                entity.Property(e => e.Equipment)
                    .HasColumnName("equipment")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FormTypeId).HasColumnName("form_type_id");

                entity.Property(e => e.JobId).HasColumnName("job_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

                entity.Property(e => e.AreaName)
                    .HasColumnName("AreaName")
                    .HasMaxLength(255);

                entity.Property(e => e.CompanyName)
                    .HasColumnName("CompanyName")
                    .HasMaxLength(255);

                entity.Property(e => e.JobName)
                    .HasColumnName("JobName")
                    .HasMaxLength(255);

                entity.Property(e => e.FormName)
                    .HasColumnName("FormName")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<FormPercentage>(entity =>
            {

                entity.HasIndex(e => e.FormComponentId)
                    .HasName("component_checklists_ibfk_2");

                entity.HasIndex(e => e.FormId)
                    .HasName("component_checklists_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.FormComponentId).HasColumnName("form_component_id");

                entity.Property(e => e.FormId).HasColumnName("form_id");

                entity.Property(e => e.SafeValue).HasColumnName("safe_value");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UnsafeValue).HasColumnName("unsafe_value");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Inspection)
                    .HasColumnName("Inspection")
                    .HasMaxLength(255);

                entity.Property(e => e.Unsafe)
                    .HasColumnName("Unsafe");

                entity.Property(e => e.Safe)
                    .HasColumnName("Safe");

                entity.Property(e => e.Percentage)
                    .HasColumnName("Percentage");
            });

            modelBuilder.Entity<FormComponent>(entity =>
            {
                entity.ToTable("form_components");

                entity.HasIndex(e => e.ComponentId)
                    .HasName("form_components_ibfk_2");

                entity.HasIndex(e => e.FormTypeId)
                    .HasName("form_components_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ComponentId).HasColumnName("component_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.FormTypeId).HasColumnName("form_type_id");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Component)
                    .WithMany(p => p.FormComponents)
                    .HasForeignKey(d => d.ComponentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("form_components_ibfk_2");

                entity.HasOne(d => d.FormType)
                    .WithMany(p => p.FormComponents)
                    .HasForeignKey(d => d.FormTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("form_components_ibfk_1");
            });

            modelBuilder.Entity<FormComponent2>(entity =>
            {

                entity.HasIndex(e => e.ComponentId)
                    .HasName("form_components_ibfk_2");

                entity.HasIndex(e => e.FormTypeId)
                    .HasName("form_components_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ComponentId).HasColumnName("component_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.FormTypeId).HasColumnName("form_type_id");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.FormName)
                    .HasColumnName("FormName")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.ComponentSlug)
                    .HasColumnName("ComponentSlug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.ComponentName)
                    .HasColumnName("ComponentName")
                    .HasMaxLength(int.MaxValue);
            });

            modelBuilder.Entity<FormType>(entity =>
            {
                entity.ToTable("form_types");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("form_types_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.File)
                    .HasColumnName("file")
                    .HasColumnType("text");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.Ord).HasColumnName("ord");

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.FormTypes)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("form_types_ibfk_1");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("items");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("items_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.File)
                    .IsRequired()
                    .HasColumnName("file")
                    .HasColumnType("text");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("items_ibfk_1");
            });

            

            modelBuilder.Entity<ItemInspection>(entity =>
            {
                entity.ToTable("item_inspections");

                entity.HasIndex(e => e.AreaId)
                    .HasName("item_inspections_ibfk_1");

                entity.HasIndex(e => e.CompanyId)
                    .HasName("item_inspections_ibfk_3");

                entity.HasIndex(e => e.ItemId)
                    .HasName("item_inspections_ibfk_2");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AreaId).HasColumnName("area_id");

                entity.Property(e => e.Barcode)
                    .IsRequired()
                    .HasColumnName("barcode")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.Brand)
                    .IsRequired()
                    .HasColumnName("brand")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("date");

                entity.Property(e => e.Inspector)
                    .HasColumnName("inspector")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Lng)
                    .HasColumnName("lng")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Model)
                    .IsRequired()
                    .HasColumnName("model")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Safetyman)
                    .HasColumnName("safetyman")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("date");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.ItemInspections)
                    .HasForeignKey(d => d.AreaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_inspections_ibfk_1");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.ItemInspections)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_inspections_ibfk_3");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemInspections)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_inspections_ibfk_2");
            });

            modelBuilder.Entity<ItemInspection2>(entity =>
            {

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AreaId).HasColumnName("area_id");

                entity.Property(e => e.Barcode)
                    .IsRequired()
                    .HasColumnName("barcode")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.Brand)
                    .IsRequired()
                    .HasColumnName("brand")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("date");

                entity.Property(e => e.Inspector)
                    .HasColumnName("inspector")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Lng)
                    .HasColumnName("lng")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Model)
                    .IsRequired()
                    .HasColumnName("model")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Safetyman)
                    .HasColumnName("safetyman")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("date");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");

                entity.Property(e => e.AreaName)
                    .HasColumnName("AreaName")
                    .HasMaxLength(255);

                entity.Property(e => e.CompanyName)
                    .HasColumnName("CompanyName")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.SlugItem)
                    .HasColumnName("SlugItem")
                    .HasMaxLength(int.MaxValue)
                    .IsUnicode(false);

                entity.HasNoKey();
            });

            modelBuilder.Entity<ItemInspectionChecklist>(entity =>
            {
                entity.ToTable("item_inspection_checklists");

                entity.HasIndex(e => e.ItemInspectionId)
                    .HasName("item_inspection_checklists_ibfk_1");

                entity.HasIndex(e => e.ItemRequirementId)
                    .HasName("item_inspection_checklists_ibfk_2");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.ItemInspectionId).HasColumnName("item_inspection_id");

                entity.Property(e => e.ItemRequirementId).HasColumnName("item_requirement_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.ItemInspection)
                    .WithMany(p => p.ItemInspectionChecklists)
                    .HasForeignKey(d => d.ItemInspectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_inspection_checklists_ibfk_1");

                entity.HasOne(d => d.ItemRequirement)
                    .WithMany(p => p.ItemInspectionChecklists)
                    .HasForeignKey(d => d.ItemRequirementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_inspection_checklists_ibfk_2");
            });

            modelBuilder.Entity<ItemInspectionChecklistAttachment>(entity =>
            {
                entity.ToTable("item_inspection_checklist_attachments");

                entity.HasIndex(e => e.ItemInspectionChecklistId)
                    .HasName("item_inspection_checklist_attachments_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.File)
                    .IsRequired()
                    .HasColumnName("file")
                    .HasColumnType("text");

                entity.Property(e => e.ItemInspectionChecklistId).HasColumnName("item_inspection_checklist_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.ItemInspectionChecklist)
                    .WithMany(p => p.ItemInspectionChecklistAttachments)
                    .HasForeignKey(d => d.ItemInspectionChecklistId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_inspection_checklist_attachments_ibfk_1");
            });

            modelBuilder.Entity<ItemRequirement>(entity =>
            {
                entity.ToTable("item_requirements");

                entity.HasIndex(e => e.ItemId)
                    .HasName("item_requirements_ibfk_1");

                entity.HasIndex(e => e.RequirementId)
                    .HasName("item_requirements_ibfk_2");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.RequirementId).HasColumnName("requirement_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemRequirements)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_requirements_ibfk_1");

                entity.HasOne(d => d.Requirement)
                    .WithMany(p => p.ItemRequirements)
                    .HasForeignKey(d => d.RequirementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("item_requirements_ibfk_2");
            });

            modelBuilder.Entity<ItemRequirement2>(entity =>
            {

                entity.HasIndex(e => e.ItemId)
                    .HasName("item_requirements_ibfk_1");

                entity.HasIndex(e => e.RequirementId)
                    .HasName("item_requirements_ibfk_2");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.ItemId).HasColumnName("item_id");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.RequirementId).HasColumnName("requirement_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.ItemName)
                    .HasColumnName("ItemName")
                    .HasMaxLength(255);

                entity.Property(e => e.RequirementSlug)
                    .HasColumnName("RequirementSlug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.RequirementName)
                    .HasColumnName("RequirementName")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("jobs");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("jobs_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("jobs_ibfk_1");
            });

            modelBuilder.Entity<ModelHasPermission>(entity =>
            {
                entity.HasKey(e => new { e.PermissionId, e.ModelId, e.ModelType })
                    .HasName("pk_model_has_permissions");

                entity.ToTable("model_has_permissions");

                entity.HasIndex(e => new { e.ModelId, e.ModelType })
                    .HasName("model_has_permissions_model_id_model_type_index");

                entity.Property(e => e.PermissionId).HasColumnName("permission_id");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.ModelType)
                    .HasColumnName("model_type")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.ModelHasPermissions)
                    .HasForeignKey(d => d.PermissionId)
                    .HasConstraintName("model_has_permissions_permission_id_foreign");
            });

            modelBuilder.Entity<ModelHasRole>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ModelId, e.ModelType })
                    .HasName("pk_model_has_roles");

                entity.ToTable("model_has_roles");

                entity.HasIndex(e => new { e.ModelId, e.ModelType })
                    .HasName("model_has_roles_model_id_model_type_index");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.ModelType)
                    .HasColumnName("model_type")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.ModelHasRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("model_has_roles_role_id_foreign");
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("permissions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.GuardName)
                    .IsRequired()
                    .HasColumnName("guard_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Refinery>(entity =>
            {
                entity.ToTable("refineries");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasColumnType("text");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Requirement>(entity =>
            {
                entity.ToTable("requirements");

                entity.HasIndex(e => e.RefineryId)
                    .HasName("requirements_ibfk_1");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Refinery)
                    .WithMany(p => p.Requirements)
                    .HasForeignKey(d => d.RefineryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("requirements_ibfk_1");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.GuardName)
                    .IsRequired()
                    .HasColumnName("guard_name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<RoleHasPermission>(entity =>
            {
                entity.HasKey(e => new { e.PermissionId, e.RoleId })
                    .HasName("pk_role_has_permissions");

                entity.ToTable("role_has_permissions");

                entity.HasIndex(e => e.RoleId)
                    .HasName("role_has_permissions_role_id_foreign");

                entity.Property(e => e.PermissionId).HasColumnName("permission_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.RoleHasPermissions)
                    .HasForeignKey(d => d.PermissionId)
                    .HasConstraintName("role_has_permissions_permission_id_foreign");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleHasPermissions)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("role_has_permissions_role_id_foreign");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.Username)
                    .HasName("users_username_unique")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EmailVerifiedAt)
                    .HasColumnName("email_verified_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Photo)
                    .HasColumnName("photo")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.RefineryId).HasColumnName("refinery_id");

                entity.Property(e => e.RememberToken)
                    .HasColumnName("remember_token")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasMaxLength(int.MaxValue);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Weather>(entity =>
            {
                entity.ToTable("weathers");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasColumnName("slug")
                    .HasColumnType("text");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
