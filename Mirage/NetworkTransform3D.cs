using Godot;
using Mirage;
using Mirage.Logging;
public partial class NetworkTransform3D : NetworkBehaviour
{
    private static readonly ILogger logger = LogFactory.GetLogger<NetworkTransform3D>();

    [Export] public Node3D _target;
    private Vector3 _previousPos = Vector3.Zero;
    private Quaternion _previousRot;
    private Vector3 _targetPos = Vector3.Zero;
    private Quaternion _targetRot;
    /*
    public static void SerializeIntoWriter(NetworkWriter writer, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        // serialize position, rotation, scale
        // note: we do NOT compress rotation.
        //       we are CPU constrained, not bandwidth constrained.
        //       the code needs to WORK for the next 5-10 years of development.
        writer.WriteVector3(position);
        writer.WriteQuaternion(rotation);
        writer.WriteVector3(scale);
    }

    public override bool SerializeSyncVars(NetworkWriter writer, bool initial)
    {
        if(initial)
        {
            SerializeIntoWriter(writer, _target.Position, _target.Quaternion, _target.Scale);
            return true;
        }
        return false;
    }

    private void DeserializeFromReader(NetworkReader reader)
    {

        // deserialize position
        _targetPos = reader.ReadVector3();
    

        // deserialize rotation & scale
        reader.ReadQuaternion();
        reader.ReadVector3();

    }

    public override void DeserializeSyncVars(NetworkReader reader, bool initial)
    {
        if (initial)
        {
            DeserializeFromReader(reader);
        }
    }
    */
    public override void _Process(double delta)
    {

        if (!Identity.IsSpawned)
            return;

        if ((this.IsServer() && Identity.Owner == null) || this.HasAuthority())
        {
            //GD.Print($"CheckChanged: {Identity.NetId}");
            if (logger.LogEnabled()) logger.Log($"CheckChanged: {Identity.NetId}");
            CheckChanged();
        }
        else
        {
            //GD.Print($"MoveTowards: {Identity.NetId}");
            if (logger.LogEnabled()) logger.Log($"MoveTowards: {Identity.NetId}");
            MoveTowards();
        }
    }

    private void CheckChanged()
    {
        var currentPos = _target.Position;
        var currentRot = _target.Quaternion;

        if (currentPos.DistanceTo(_previousPos) > 0.01f
            || currentRot.AngleTo(_previousRot) > 0.01f
            )
        {
            if (this.IsServer())
            {
                SendUpdate(currentPos, currentRot);
            }
            else
            {
                SendUpdateRelayed(currentPos, currentRot);
            }
            _previousPos = currentPos;
            _previousRot = currentRot;
        }
    }

    [ServerRpc]
    private void SendUpdateRelayed(Vector3 pos, Quaternion rot)
    {
        //GD.Print($"RPC ToServer: {Identity.NetId}, {pos} {rot}");
        if (logger.LogEnabled()) logger.Log($"RPC ToServer: {Identity.NetId}, {pos} {rot}");
        SendUpdate(pos, rot);
    }
    [ClientRpc]
    private void SendUpdate(Vector3 pos, Quaternion rot)
    {
        //GD.Print($"RPC ToClient: {Identity.NetId}, {pos} {rot}");
        if (logger.LogEnabled()) logger.Log($"RPC ToClient: {Identity.NetId}, {pos} {rot}");
        _targetPos = pos;
        //_targetRot = rot;
    }

    private void MoveTowards()
    {
        //GD.Print($"MoveTowards: {Identity.NetId}, from[{_target.Position},{_target.Quaternion}] to[{_targetPos},{_targetRot}]");
        if (logger.LogEnabled()) logger.Log($"MoveTowards: {Identity.NetId}, from[{_target.Position},{_target.Quaternion}] to[{_targetPos},{_targetRot}]");
        _target.Position = _targetPos;
        //_target.Quaternion = _targetRot;
    }
}