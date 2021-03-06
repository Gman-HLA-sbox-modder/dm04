using Sandbox;

[Library( "dm04_crossbow", Title = "Crossbow" )]
[Hammer.EditorModel("models/worldmodels/w_crossbow_reference.vmdl")]
[Hammer.EntityTool( "Crossbow", "DM:04" )]
partial class Crossbow : BaseDmWeapon
{ 
	public override string ViewModelPath => "models/viewmodels/crossbow/crossbow_reference.vmdl";

	public override float PrimaryRate => 1;

	public override float ReloadTime => 1.0f;
	public override int Bucket => 3;
	public override int ClipSize => 1;
	public override AmmoType AmmoType => AmmoType.Crossbow;

	[Net]
	public bool Zoomed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = 1;
		SetModel("models/worldmodels/w_crossbow_reference.vmdl");
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 1 ) )
		{
			return;
		}

		ShootEffects();

		if ( IsServer )
		using ( Prediction.Off() )
		{
			var bolt = new CrossbowBolt();
			bolt.Position = Owner.EyePos;
			bolt.Rotation = Owner.EyeRot;
			bolt.Owner = Owner;
			bolt.Velocity = Owner.EyeRot.Forward * 100;
		}

		ShootBullet( 0.1f, 1.5f, 100.0f, 3.0f );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Zoomed = Input.Down( InputButton.Attack2 );

		if (AmmoClip == 0) 
		{
			Reload();
		}
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Zoomed )
		{
			camSetup.FieldOfView = 20;
		}
	}

	public override void BuildInput( InputBuilder owner ) 
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}
}
