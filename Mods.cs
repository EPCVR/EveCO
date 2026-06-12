using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EveCO
{
    public class Mods : MonoBehaviour
    {
        static GorillaTagger player2 => GorillaTagger.Instance;
        static GorillaLocomotion.GTPlayer player => GorillaLocomotion.GTPlayer.Instance;

        public static void Speedboost()
        {
            player.jumpMultiplier = 12;
            player.maxJumpSpeed = 12;
        }

        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                player.transform.position += GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 10;
                player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void GhostMonke()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
                player2.offlineVRRig.enabled = false;

            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
                player2.offlineVRRig.enabled = true;
        }
    }
}
