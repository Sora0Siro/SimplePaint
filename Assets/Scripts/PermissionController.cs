using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif


public class PermissionController : MonoBehaviour
{
    void OnEnable()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
    }
}