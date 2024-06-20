using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
  [Header("References")]
  public Design design;
  public Vector3 velocity;
  public GameObject centerEyeAnchor;
  public ParticleSystem micBubbles;
  public GameObject leftHand;
  public Rigidbody lFluidDyn;
  public Rigidbody lSpring;
  public Rigidbody llSpring;
  public Transform lNormal;
  public GameObject rightHand;
  public Rigidbody rFluidDyn;
  public Rigidbody rSpring;
  public Rigidbody rrSpring;
  public Transform rNormal;
  public Transform bgLoop;

  Color ogFogColor;
  Vector3 oldLHandPos;
  Vector3 oldRHandPos;
  Vector3 newVelocity;
  Vector3 smoothVelocity;
  Vector3 lPullBoost;
  Vector3 rPullBoost;

  private void Start()
  {
    design.lShrimp = 0;
    design.rShrimp = 0;

    oldRHandPos = design.rHandPos;
    oldLHandPos = design.lHandPos;

    ogFogColor = centerEyeAnchor.GetComponent<Camera>().backgroundColor;

    AudioSource lAudio = lSpring.transform.GetComponent<AudioSource>();
    lAudio.PlayDelayed(1);
  }

  private void Update()
  {
    // Swimming
    Vector3 lHandVel = leftHand.transform.localPosition - oldLHandPos;
    Vector3 rHandVel = rightHand.transform.localPosition - oldRHandPos;

    // strength add for each hand (1 hand = 0.5f; 2 hand = 1;)
    float overallStrength = (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) + OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch)) * 0.5f;
    float lStrength = Vector3.Angle(lNormal.position - leftHand.transform.position, lHandVel) / 180;
    float rStrength = Vector3.Angle(rNormal.position - rightHand.transform.position, rHandVel) / 180;

    // Coast ? 0.5 0.025
    float lDrag = Mathf.Abs(lStrength - 0.5f) * 2;
    float rDrag = Mathf.Abs(rStrength - 0.5f) * 2;

    float drag = Mathf.Lerp(0.025f, 1f, (lDrag + rDrag) * (lDrag + rDrag) * 0.5f);

    // x^2 or x^3 or... lerp between x^2 and linear
    lStrength *= lStrength * lStrength * OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
    rStrength *= rStrength * rStrength * OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);

    newVelocity = ((lHandVel * lStrength) + (rHandVel * rStrength)) * design.accel * overallStrength;
    oldLHandPos = leftHand.transform.localPosition;
    oldRHandPos = rightHand.transform.localPosition;

    // replace smoothvelocity with mass?
    smoothVelocity = Vector3.Lerp(smoothVelocity, newVelocity - lPullBoost - rPullBoost, 0.5f * Time.deltaTime * 8);

    velocity -= smoothVelocity;

    velocity *= 1 - (drag * Time.deltaTime);
    velocity = Vector3.ClampMagnitude(velocity, design.maxVel);

    if (overallStrength > 0)
    {
      Debug.Log(rHandVel);
    }

    if (!design.grappling)
      transform.position += velocity * Time.deltaTime;
    else
      transform.position += design.netVelocity * Time.deltaTime;

    // Haptics
    OVRInput.SetControllerVibration(0, lDrag * ((lHandVel / Time.deltaTime).magnitude / design.maxVel) * 0.333f, OVRInput.Controller.LTouch);
    OVRInput.SetControllerVibration(0, rDrag * ((rHandVel / Time.deltaTime).magnitude / design.maxVel) * 0.333f, OVRInput.Controller.RTouch);

    // Storing position Vectors in a ScriptableObject
    design.playerPos = transform.position;

    design.lHandPos = leftHand.transform.position;
    design.lSpring = lSpring.transform.position;
    design.llSpring = llSpring.transform.position;

    design.rHandPos = rightHand.transform.position;
    design.rSpring = rSpring.transform.position;
    design.rrSpring = rrSpring.transform.position;

    // Bubbles (respirator)
    var emitBubbles = micBubbles.emission;
    float micValue = MicInput.MicLoudness * 4;
    emitBubbles.rateOverTime = micValue * 10;

    // Lock BG ambient loop rotation
    bgLoop.rotation = Quaternion.identity;
    bgLoop.position = Vector3.Lerp(bgLoop.position, design.playerPos, 0.5f * Time.deltaTime * design.bgLoopLerp);

    // Fog color shift
    float centerDist = Vector3.Distance(design.playerPos, Vector3.zero) / design.fogFallOff;
    Color sharedColor = Color.Lerp(ogFogColor, Color.black, centerDist);
    //centerEyeAnchor.GetComponent<Camera>().backgroundColor = sharedColor;
    //centerEyeAnchor.GetComponent<SSMS.SSMSGlobalFog>().fogColor = sharedColor;

    // Shrimp vel color shift 
    // !switch to sqrMagnitude
    float lMag = lSpring.velocity.magnitude;
    if (lMag > design.shrMinVel)
      design.shrLColorShift = Color.Lerp(Color.magenta, Color.red, lMag / design.shrMaxVel);
    else
      design.shrLColorShift = Color.cyan;

    float rMag = rSpring.velocity.magnitude;
    if (rMag > design.shrMinVel)
      design.shrRColorShift = Color.Lerp(Color.magenta, Color.red, rMag / design.shrMaxVel);
    else
      design.shrRColorShift = Color.cyan;

    design.lSpringVel = lSpring.velocity;
    design.rSpringVel = rSpring.velocity;

    AudioSource lAudio = lSpring.transform.GetComponent<AudioSource>();
    AudioSource rAudio = rSpring.transform.GetComponent<AudioSource>();

    lAudio.volume = (lMag / design.shrMaxVel) * (design.lShrimp / design.shrimpPerHand);
    rAudio.volume = (rMag / design.shrMaxVel) * (design.rShrimp / design.shrimpPerHand);
  }

  private void FixedUpdate()
  {
    // SPRINGS
    // Fluid Dynamics (consider dynamic drag) accomadate tracking loss and start
    lFluidDyn.AddForce(-design.fluidDynStr * (lFluidDyn.position - design.lHandPos));
    rFluidDyn.AddForce(-design.fluidDynStr * (rFluidDyn.position - design.rHandPos));


    // Shrimp springs
    if (design.lStick != 0)
    {
      float lFalloff = Mathf.Clamp01(Vector3.Distance(leftHand.transform.position, lSpring.position) / design.fallOff);
      if (lSpring.velocity.magnitude > design.shrMinVel)
      {
        lSpring.AddForce(Vector3.Normalize(lSpring.velocity) * design.combatMod * (design.lTrigger - lFalloff));
        lPullBoost = Vector3.zero;
      }
      else
      {
        //lSpring.AddForce(leftHand.transform.rotation * Vector3.forward * design.lTrigger);
        //lPullBoost = lSpring.velocity * design.lTrigger / Time.deltaTime;
      }

      lSpring.AddForce(-design.hSpringStr * (lSpring.position - design.lHandPos) * (1 - design.lTrigger + (lFalloff * design.lTrigger)));
      lSpring.velocity = Vector3.ClampMagnitude(lSpring.velocity, design.shrMaxVel);
      llSpring.AddForce(-design.sSpringStr * (llSpring.position - design.lSpring));
    }
    else if (design.lShrimp == 0)
    {
      lSpring.position = leftHand.transform.position;
      lSpring.velocity = Vector3.zero;
      llSpring.position = leftHand.transform.position;
      llSpring.velocity = Vector3.zero;
    }

    if (design.rStick != 0)
    {
      float rFalloff = Mathf.Clamp01(Vector3.Distance(rightHand.transform.position, rSpring.position) / design.fallOff);
      if (rSpring.velocity.magnitude > design.shrMinVel)
      {
        rSpring.AddForce(Vector3.Normalize(rSpring.velocity) * design.combatMod * (design.rTrigger - rFalloff));
        rPullBoost = Vector3.zero;
      }
      else
      {
        //rSpring.AddForce(rightHand.transform.rotation * Vector3.forward * design.rTrigger);
        //rPullBoost = rSpring.velocity * design.rTrigger / Time.deltaTime;
      }
      rSpring.AddForce(-design.hSpringStr * (rSpring.position - design.rHandPos) * (1 - design.rTrigger + (rFalloff * design.rTrigger)));
      rSpring.velocity = Vector3.ClampMagnitude(rSpring.velocity, design.shrMaxVel);
      rrSpring.AddForce(-design.sSpringStr * (rrSpring.position - design.rSpring));
    }
    else if (design.rShrimp == 0)
    {
      rSpring.position = rightHand.transform.position;
      rSpring.velocity = Vector3.zero;
      rrSpring.position = rightHand.transform.position;
      rrSpring.velocity = Vector3.zero;
    }
  }
}
