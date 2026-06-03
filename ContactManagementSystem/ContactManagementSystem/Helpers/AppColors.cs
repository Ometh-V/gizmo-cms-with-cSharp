using System.Drawing;

namespace ContactManagementSystem.Helpers
{
    public static class AppColors
    {
        // ── Backgrounds ──────────────────────────────────────────────────────
        public static readonly Color Background = Color.FromArgb(22, 22, 30);   // Darkest — form base
        public static readonly Color Surface = Color.FromArgb(30, 30, 42);   // Panels / cards
        public static readonly Color SurfaceLight = Color.FromArgb(38, 38, 52);   // Inputs / hover states
        public static readonly Color Header = Color.FromArgb(25, 25, 35);   // Top bar
        public static readonly Color Sidebar = Color.FromArgb(28, 28, 38);   // Left nav

        // ── Interactive ──────────────────────────────────────────────────────
        public static readonly Color NavActive = Color.FromArgb(40, 70, 130);  // Selected nav button
        public static readonly Color NavHover = Color.FromArgb(35, 35, 50);   // Hovered nav button
        public static readonly Color Primary = Color.FromArgb(70, 120, 220);  // Accent / links
        public static readonly Color Danger = Color.FromArgb(190, 55, 55);   // Delete button

        // ── Text ─────────────────────────────────────────────────────────────
        public static readonly Color TextPrimary = Color.FromArgb(235, 235, 245); // Main text
        public static readonly Color TextSecondary = Color.FromArgb(130, 130, 155); // Subtitles / labels

        // ── Structural ───────────────────────────────────────────────────────
        public static readonly Color Border = Color.FromArgb(45, 45, 62);   // Dividers
        public static readonly Color ListSelected = Color.FromArgb(45, 78, 145);  // Selected contact row
        public static readonly Color ListHover = Color.FromArgb(38, 38, 52);   // Hovered contact row

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