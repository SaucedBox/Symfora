namespace TripleS.Scripting {
    public interface IInteractableEnt {

        public bool DisableInteraction { get; }
        public void OnInteraction() { }
    }
}
