using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Zoom
    {
        public static void Postfix(HudManager __instance)
        {
            if (ModeHandler.isMode(ModeId.Default) && MapOptions.MapOption.MouseZoom && PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    if (Camera.main.orthographicSize > 1.0f)
                    {
                        Camera.main.orthographicSize /= 1.5f;
                        __instance.transform.localScale /= 1.5f;
                        __instance.UICamera.orthographicSize /= 1.5f;
                        HudManager.Instance.TaskStuff.SetActive(false);
                    }

                    else if (Camera.main.orthographicSize > 3.0f)
                    {
                        Camera.main.orthographicSize /= 1.5f;
                        __instance.transform.localScale /= 1.5f;
                        __instance.UICamera.orthographicSize /= 1.5f;
                    }
                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (Camera.main.orthographicSize < 18.0f)
                        {
                            Camera.main.orthographicSize *= 1.5f;
                            __instance.transform.localScale *= 1.5f;
                            __instance.UICamera.orthographicSize *= 1.5f;
                        }
                    }
                }
                if (ModeHandler.isMode(ModeId.Default))
                {
                    if (Camera.main.orthographicSize != 3.0f)
                    {
                        HudManager.Instance.TaskStuff.SetActive(false);
                        ModManager.Instance.ModStamp.gameObject.SetActive(false);
                        if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(false);
                    }
                    else
                    {
                        HudManager.Instance.TaskStuff.SetActive(true);
                        ModManager.Instance.ModStamp.gameObject.SetActive(true);
                        if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(true);
                    }
                }
                CreateFlag.NewFlag("Zoom");
            }
            else
            {
                CreateFlag.Run(() =>
                {
                    Camera.main.orthographicSize = 3.0f;
                    HudManager.Instance.UICamera.orthographicSize = 3.0f;
                    HudManager.Instance.transform.localScale = Vector3.one;
                    if (MeetingHud.Instance != null) MeetingHud.Instance.transform.localScale = Vector3.one;
                    HudManager.Instance.Chat.transform.localScale = Vector3.one;
                    if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(true);
                }, "Zoom");
            }
        }
    }
}
