using Orchard.Data.Migration;

namespace Orchard.Projection.RoleSecurity {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("RoleSecurityRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<bool>("AnonymousCanView")
                    .Column<int>("ContentItemRecord_id")
                    .Column<int>("Permission_id")
                );

            SchemaBuilder.CreateTable("RolesSecuritysRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Role_id")
                    .Column<int>("RoleSecurity_id")
                    .Column<int>("RoleRecord_Id")
                    .Column<int>("RoleSecurityRecord_id")
                );

            return 1;
        }
    }
}
