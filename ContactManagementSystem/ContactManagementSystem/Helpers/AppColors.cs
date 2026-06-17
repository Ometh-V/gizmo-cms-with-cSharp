using System.Drawing;

namespace ContactManagementSystem.Helpers
{
    public static class AppColors
    {
        // ── Backgrounds ──────────────────────────────────────────────────────
        public static readonly Color Background = Color.FromArgb(22, 22, 30);
        public static readonly Color Surface = Color.FromArgb(30, 30, 42);
        public static readonly Color SurfaceLight = Color.FromArgb(38, 38, 52);
        public static readonly Color Header = Color.FromArgb(25, 25, 35);
        public static readonly Color Sidebar = Color.FromArgb(28, 28, 38);

        // ── Interactive ──────────────────────────────────────────────────────
        public static readonly Color NavActive = Color.FromArgb(40, 70, 130);
        public static readonly Color NavHover = Color.FromArgb(35, 35, 50);
        public static readonly Color Primary = Color.FromArgb(70, 120, 220);
        public static readonly Color Danger = Color.FromArgb(190, 55, 55);

        // ── Text ─────────────────────────────────────────────────────────────
        public static readonly Color TextPrimary = Color.FromArgb(235, 235, 245);
        public static readonly Color TextSecondary = Color.FromArgb(130, 130, 155);

        // ── Structural ───────────────────────────────────────────────────────
        public static readonly Color Border = Color.FromArgb(45, 45, 62);
        public static readonly Color ListSelected = Color.FromArgb(45, 78, 145);
        public static readonly Color ListHover = Color.FromArgb(38, 38, 52);

        // ── Avatar circle colours (assigned round-robin per contact) ─────────
        public static readonly Color[] AvatarColors =
        {
            Color.FromArgb(70,  120, 200),  // Blue
            Color.FromArgb(70,  155, 100),  // Green
            Color.FromArgb(195, 115, 55),   // Orange
            Color.FromArgb(155, 75,  155),  // Purple
            Color.FromArgb(70,  160, 170),  // Teal
            Color.FromArgb(190, 80,  80)    // Red
        };
    }
}