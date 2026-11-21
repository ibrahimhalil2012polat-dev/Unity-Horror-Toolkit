namespace HorrorToolkit
{
    /// <summary>Implemented by scene components that respond to a player interaction.</summary>
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
    }
}
