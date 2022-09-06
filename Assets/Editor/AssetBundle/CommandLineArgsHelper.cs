using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class CustomExtensionHelpers {
    public static T AssertArgumentNotNull<T>(this T argument, string argumentName) {
        if (argument == null) {
            throw new ArgumentNullException(argumentName);
        }

        return argument;
    }

    /// <summary>
    /// BBernard
    /// Supports exception-safe retrieval of values from a Dictionary.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static V GetValueOrDefault<K, V>(
        this IReadOnlyDictionary<K, V> dictionary,
        K key,
        V defaultValue = default(V)
    ) {
        dictionary.AssertArgumentNotNull(nameof(dictionary));
        key.AssertArgumentNotNull(nameof(key));

        var getSucc = dictionary.TryGetValue(key, out var value);
        var result = getSucc
            ? (value == null ? defaultValue : value)
            : defaultValue;
        return result;
    }
}

public class CommandLineArgsHelper {
    public IReadOnlyDictionary<string, object> Parameters { get; }

    public CommandLineArgsHelper()
        : this(Environment.GetCommandLineArgs()) {
    }

    public CommandLineArgsHelper(string[] argsArray) {
        this.Parameters =
            new ReadOnlyDictionary<string, object>((IDictionary<string, object>)this.parseCMDArgs(argsArray));
    }

    Dictionary<string, object> parseCMDArgs(string[] strings) {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        String prop = null;
        foreach (var arg in strings) {
            if (arg.StartsWith("-")) {
                var substring = arg.Substring(1);
                if (prop != null && !dictionary.ContainsKey(prop)) {
                    dictionary.Add(prop, null);
                }

                prop = substring;
            }
            else {
                if (prop != null && !dictionary.ContainsKey(prop)) {
                    dictionary.Add(prop, arg);
                    prop = null;
                }
            }
        }

        if (prop != null && !dictionary.ContainsKey(prop)) {
            dictionary.Add(prop, null);
        }

        return dictionary;
    }

    public string this[string name] => GetValue(name, "");

    public V GetValue<V>(string name, V val) {
        return (V)Parameters.GetValueOrDefault(name, val);
    }

    public bool GetBool(string name, bool defVal=true) {
        string valueOrDefault = (string)Parameters.GetValueOrDefault(name);
        return bool.Parse(valueOrDefault ?? defVal.ToString());
    }

    public string GetString(string name, string defVal = "") {
        return GetValue(name, defVal);
        return (string)(Parameters.GetValueOrDefault(name) ?? "");
    }
}