using Sandbox;


[Library( "dm04_shotgun", Title = "Shotgun" )]
[Hammer.EditorModel("models/worldmodels/shotgun_reference.vmdl")]
[Hammer.EntityTool( "Shotgun", "DM:04" )]
partial class Shotgun : BaseDmWeapon
{ 
	public override string ViewModelPath => "models/viewmodels/shotgun/shotgun_reference.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int ClipSize => 6;
	public override float ReloadTime => 0.5f;
	public override int Bucket => 3;

	private int TotalShells = 7;

	public override void Spawn()
	{
		base.Spawn();

		SetModel("models/worldmodels/shotgun_reference.vmdl");  

		AmmoClip = ClipSize;
	}

	public override void AttackPrimary() 
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "shotgun_fire7" );

		//
		// Shoot the bullets
		//
		for ( int i = 0; i <= TotalShells; i++ )
			ShootBullet( 0.15f, 0.3f, 9.0f, 3.0f);
		
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		if ( !TakeAmmo( 2 ) )
		{
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();
		PlaySound( "shotgun_dbl_fire" );

		//
		// Shoot the bullets
		//
		for ( int i = 0; i <= TotalShells * 2; i++ )
			ShootBullet( 0.15f, 0.3f, 9.0f, 3.0f );
	}

	public override void Simulate(Client cl) 
	{
		if (AmmoClip == 0 && TimeSincePrimaryAttack > 0.5f)
		{
			Reload();
			StartReloadEffects();
		}
		
		base.Simulate(cl);
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(1.0f, 1.5f, 2.0f);
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimBool( "altfire", true );
		CrosshairPanel?.CreateEvent( "fire" );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(3.0f, 3.0f, 3.0f);
		}
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is DeathmatchPlayer player )
		{
			var ammo = player.TakeAmmo( AmmoType, 1 );
			if ( ammo == 0 )
				return;

			if ( AmmoClip < ClipSize )
			{
				Reload();
			}
			else
			{
				FinishReload();
			}

			AmmoClip += ammo;
		}
	}

	[ClientRpc]
	public override void StartReloadEffects() 
	{
		base.StartReloadEffects();
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_end", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 1 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
