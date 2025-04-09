using UnityEngine;
using System.Reflection;
using UnityEditor;

public static class SCR_InspectorLockUtility
{
    public static void SetInspectorLock(bool isLocked)
    {
        var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        var windows = Resources.FindObjectsOfTypeAll(inspectorType);

        foreach (var window in windows)
        {
            var property = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property?.SetValue(window, isLocked, null);

            var repaint = inspectorType.GetMethod("Repaint", BindingFlags.Instance | BindingFlags.Public);
            repaint?.Invoke(window, null);
        }
    }
}
