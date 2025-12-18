using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glense.VideoCatalogue.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    upload_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    uploader_id = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    video_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false),
                    like_count = table.Column<int>(type: "integer", nullable: false),
                    dislike_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.id);
                    table.ForeignKey(
                        name: "FK_Videos_Users_uploader_id",
                        column: x => x.uploader_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    creator_id = table.Column<int>(type: "integer", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.id);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_creator_id",
                        column: x => x.creator_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistVideos",
                columns: table => new
                {
                    playlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistVideos", x => new { x.playlist_id, x.video_id });
                    table.ForeignKey(
                        name: "FK_PlaylistVideos_Playlists_playlist_id",
                        column: x => x.playlist_id,
                        principalTable: "Playlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistVideos_Videos_video_id",
                        column: x => x.video_id,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    subscriber_id = table.Column<int>(type: "integer", nullable: false),
                    subscribed_to_id = table.Column<int>(type: "integer", nullable: false),
                    subscription_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => new { x.subscriber_id, x.subscribed_to_id });
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_subscriber_id",
                        column: x => x.subscriber_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_subscribed_to_id",
                        column: x => x.subscribed_to_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoLikes",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_liked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoLikes", x => new { x.user_id, x.video_id });
                    table.ForeignKey(
                        name: "FK_VideoLikes_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoLikes_Videos_video_id",
                        column: x => x.video_id,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistVideos_video_id",
                table: "PlaylistVideos",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_creator_id",
                table: "Playlists",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_subscribed_to_id",
                table: "Subscriptions",
                column: "subscribed_to_id");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLikes_video_id",
                table: "VideoLikes",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_uploader_id",
                table: "Videos",
                column: "uploader_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlaylistVideos");
            migrationBuilder.DropTable(name: "VideoLikes");
            migrationBuilder.DropTable(name: "Subscriptions");
            migrationBuilder.DropTable(name: "Playlists");
            migrationBuilder.DropTable(name: "Videos");
        }
    }
}
