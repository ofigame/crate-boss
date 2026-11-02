using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Validate Layout (Ana Belge §3.5): runs the Windows test build with "-layoutcheck", which builds every
    /// level as in play and writes Docs/YERLESIM_RAPORU.md. The checks need the real scene (district, crane, camera), so they
    /// run in the player, not in the editor. Build the player first (Setup builds it; build-crateboss.sh layout does both).
    /// </summary>
    public static class LayoutCheckMenu
    {
        public const string Report = "Docs/YERLESIM_RAPORU.md";

        [MenuItem("Tools/Crate Boss/Validate Layout")]
        public static void Run()
        {
            string exe = Path.GetFullPath("Builds/Windows/CrateBoss.exe");
            if (!File.Exists(exe))
            {
                Debug.LogError("Validate Layout: build the Windows player first (Builds/Windows/CrateBoss.exe).");
                return;
            }
            var args = $"-screen-width 540 -screen-height 960 -screen-fullscreen 0 -layoutcheck \"{Path.GetFullPath(Report)}\"";
            using (var p = Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = false }))
            {
                p.WaitForExit(15 * 60 * 1000);
            }
            Debug.Log($"Validate Layout: report in {Report}");
            AssetDatabase.Refresh();
        }
    }
}
