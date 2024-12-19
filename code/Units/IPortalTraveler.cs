public interface IPortalTraveler
{
	public Transform TravelerTransform { get; set; }
	public void TeleportTo( Transform destinationTransform );
}