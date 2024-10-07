using Microsoft.Xna.Framework.Content;

namespace TripleS.UI {
    public interface IUIDesigner {

        public void LoadFonts(ContentManager cm) { }
        public void Load(int menu, out string[] whitelist) {
            whitelist = new string[0];
        }
        public void Update() { }
    }
}