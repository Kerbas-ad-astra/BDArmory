//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18449
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
namespace BahaTurret
{
	public class ModuleTurret : PartModule
	{
		[KSPField]
		public string pitchTransformName = "pitchTransform";
		public Transform pitchTransform;

		[KSPField]
		public string yawTransformName = "yawTransform";
		public Transform yawTransform;


		Transform referenceTransform; //set this to gun's fireTransform

		[KSPField]
		public float pitchSpeedDPS;
		[KSPField]
		public float yawSpeedDPS;


		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Max Pitch"),
		 UI_FloatRange(minValue = 0f, maxValue = 60f, stepIncrement = 1f, scene = UI_Scene.All)]
		public float maxPitch;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Min Pitch"),
		 UI_FloatRange(minValue = 1f, maxValue = 0f, stepIncrement = 1f, scene = UI_Scene.All)]
		public float minPitch;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Yaw Range"),
		 UI_FloatRange(minValue = 1f, maxValue = 60f, stepIncrement = 1f, scene = UI_Scene.All)]
		public float yawRange;

		[KSPField(isPersistant = true)]
		public float minPitchLimit = 400;
		[KSPField(isPersistant = true)]
		public float maxPitchLimit = 400;
		[KSPField(isPersistant = true)]
		public float yawRangeLimit = 400;

		[KSPField]
		public bool smoothRotation = false;
		[KSPField]
		public float smoothMultiplier = 10;

		float pitchTargetOffset;
		float yawTargetOffset;



		public override void OnStart (StartState state)
		{
			base.OnStart (state);

			SetupTweakables();


			pitchTransform = part.FindModelTransform(pitchTransformName);
			yawTransform = part.FindModelTransform (yawTransformName);

			if(!referenceTransform)
			{
				SetReferenceTransform(pitchTransform);
			}
		}

		public void AimToTarget(Vector3 targetPosition)
		{
			if(!yawTransform)
			{
				return;
			}
			
			Vector3 localTargetYaw = yawTransform.parent.InverseTransformPoint(targetPosition - (yawTargetOffset * pitchTransform.right));
			Vector3 targetYaw = Vector3.ProjectOnPlane(localTargetYaw, Vector3.up);
			float targetYawAngle = VectorUtils.SignedAngle(Vector3.forward, targetYaw, Vector3.right);
			targetYawAngle = Mathf.Clamp(targetYawAngle, -yawRange/2, yawRange/2);
			
			Vector3 localTargetPitch = pitchTransform.parent.InverseTransformPoint(targetPosition - (pitchTargetOffset * pitchTransform.up));
			localTargetPitch.z = Mathf.Abs(localTargetPitch.z);//prevents from aiming wonky if target is behind
			Vector3 targetPitch = Vector3.ProjectOnPlane(localTargetPitch, Vector3.right);
			float targetPitchAngle = VectorUtils.SignedAngle(Vector3.forward, targetPitch, Vector3.up);
			targetPitchAngle = Mathf.Clamp(targetPitchAngle, minPitch, maxPitch);
			
			float yawSpeed;
			float pitchSpeed;
			if(smoothRotation)
			{
				float yawOffset = Vector3.Angle(yawTransform.parent.InverseTransformDirection(yawTransform.forward), targetYaw);
				float pitchOffset = Vector3.Angle(pitchTransform.parent.InverseTransformDirection(pitchTransform.forward), targetPitch);
				
				yawSpeed = Mathf.Clamp(yawOffset*smoothMultiplier, 1f, yawSpeedDPS)*Time.deltaTime;
				pitchSpeed = Mathf.Clamp(pitchOffset*smoothMultiplier, 1f, pitchSpeedDPS)*Time.deltaTime;
			}
			else
			{
				yawSpeed = yawSpeedDPS*Time.deltaTime;
				pitchSpeed = pitchSpeedDPS*Time.deltaTime;
			}
			
			yawTransform.localRotation = Quaternion.RotateTowards(yawTransform.localRotation, Quaternion.Euler(0,targetYawAngle,0), yawSpeed);
			pitchTransform.localRotation = Quaternion.RotateTowards(pitchTransform.localRotation, Quaternion.Euler(-targetPitchAngle, 0, 0), pitchSpeed);
		}

		public bool ReturnTurret()
		{
			if(!yawTransform)
			{
				return false;
			}
			yawTransform.localRotation = Quaternion.RotateTowards(yawTransform.localRotation, Quaternion.identity, yawSpeedDPS*Time.deltaTime);
			pitchTransform.localRotation = Quaternion.RotateTowards(pitchTransform.localRotation, Quaternion.identity, pitchSpeedDPS*Time.deltaTime);
			
			if(yawTransform.localRotation == Quaternion.identity && pitchTransform.localRotation == Quaternion.identity)
			{
				return true;
			}
			else
			{
				return false;
			}
		}


		public bool TargetInRange(Vector3 targetPosition, float thresholdDegrees, float maxDistance)
		{
			if(!pitchTransform)
			{
				return false;
			}
			bool withinView = Vector3.Angle(targetPosition-pitchTransform.position, pitchTransform.forward) < thresholdDegrees;
			bool withinDistance = (targetPosition-pitchTransform.position).sqrMagnitude < Mathf.Pow(maxDistance, 2);
			return (withinView && withinDistance);
		}

		public void SetReferenceTransform(Transform t)
		{
			referenceTransform = t;
			pitchTargetOffset = pitchTransform.InverseTransformPoint(referenceTransform.position).y;
			yawTargetOffset = yawTransform.InverseTransformPoint(referenceTransform.position).x;
		}

		void SetupTweakables()
		{
			var minPitchRange = (UI_FloatRange) Fields["minPitch"].uiControlEditor;
			if(minPitchLimit > 90)
			{
				minPitchLimit = minPitch;
			}
			if(minPitchLimit == 0)
			{
				Fields["minPitch"].guiActiveEditor = false;	
			}
			minPitchRange.minValue = minPitchLimit;
			minPitchRange.maxValue = 0;
			
			var maxPitchRange = (UI_FloatRange) Fields["maxPitch"].uiControlEditor;
			if(maxPitchLimit > 90)
			{
				maxPitchLimit = maxPitch;
			}
			if(maxPitchLimit == 0)
			{
				Fields["maxPitch"].guiActiveEditor = false;	
			}
			maxPitchRange.maxValue = maxPitchLimit;
			maxPitchRange.minValue = 0;
			
			var yawRangeEd = (UI_FloatRange) Fields["yawRange"].uiControlEditor;
			if(yawRangeLimit > 360)
			{
				yawRangeLimit = yawRange;	
			}
			
			if(yawRangeLimit == 0)
			{
				Fields["yawRange"].guiActiveEditor = false;
				/*
				onlyFireInRange = false;
				Fields["onlyFireInRange"].guiActiveEditor = false;
				*/
			}
			else if(yawRangeLimit < 0)
			{
				yawRangeEd.minValue = 0;
				yawRangeEd.maxValue = 360;
				
				if(yawRange < 0) yawRange = 360;
			}
			else
			{
				yawRangeEd.minValue = 0;
				yawRangeEd.maxValue = yawRangeLimit;
			}
			
		}
	}
}

