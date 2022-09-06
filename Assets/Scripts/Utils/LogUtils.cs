using System;
using UnityEngine;
using System.Collections.Generic;

public static class LogUtils
{
    public enum Loglevels
    {
        /// <summary>
        /// All message will be logged.
        /// </summary>
        All,

        /// <summary>
        /// Only Informations and above will be logged.
        /// </summary>
        Information,

        /// <summary>
        /// Only Warnings and above will be logged.
        /// </summary>
        Warning,

        /// <summary>
        /// Only Errors and above will be logged.
        /// </summary>
        Error,

        /// <summary>
        /// Only Exceptions will be logged.
        /// </summary>
        Exception,

        /// <summary>
        /// No logging will be occur.
        /// </summary>
        None
    }

    public static Loglevels Level = Loglevels.All;
    static string FormatVerbose = "V [{0}]: {1}";
    static string FormatInfo = "I [{0}]: {1}";
    static string FormatWarn = "W [{0}]: {1}";
    static string FormatErr = "Err [{0}]: {1}";

    public static void V(object verb)
    {
        V("", verb);
    }

    public static void V(string division, object verb)
    {
        if (Level <= Loglevels.All)
        {
            if (verb == null)
            {
                Debug.LogWarning("Log null object");
                return;
            }
            if (division == "")
            {
                Debug.Log(verb.ToString());
            }
            else
            {
                Debug.Log(string.Format(FormatVerbose, division, verb.ToString()));
            }
        }
    }

    public static void I(object info)
    {
        I("", info);
    }

    public static void I(string division, object info)
    {
        if (Level <= Loglevels.Information)
        {
            if (info == null)
            {
                Debug.LogWarning("Log null object");
                return;
            }
            if (division == "")
            {
                Debug.Log(info.ToString());
            }
            else
            {
                Debug.Log(string.Format(FormatInfo, division, info.ToString()));
            }
        }
    }

    public static void W(object warn)
    {
        W("", warn);
    }

    public static void W(string division, object warn)
    {
        if (Level <= Loglevels.Warning)
        {
            if (warn == null)
            {
                Debug.LogWarning("Log null object");
                return;
            }
            if (division == "")
            {
                Debug.LogWarning(warn.ToString());
            }
            else
            {
                Debug.LogWarning(string.Format(FormatWarn, division, warn.ToString()));
            }
        }
    }

    public static void E(object err)
    {
        E("", err);
    }

    public static void E(string division, object err)
    {
        if (Level <= Loglevels.Error)
        {
            if (err == null)
            {
                Debug.LogWarning("Log null object");
                return;
            }
            if (division == "")
            {
                Debug.LogError(err.ToString());
            }
            else
            {
                Debug.LogError(string.Format(FormatErr, division, err.ToString()));
            }
        }
    }
}