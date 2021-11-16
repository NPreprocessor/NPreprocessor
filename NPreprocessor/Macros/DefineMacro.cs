using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class DefineMacro : IMacro
    {
        private Dictionary<string, string> _mappings = new Dictionary<string, string>();
        private Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>();
        private Dictionary<string, Regex> _startRegexes = new Dictionary<string, Regex>();
        private Dictionary<string, bool> _hasParameters = new Dictionary<string, bool>();

        public DefineMacro()
        {
        }

        public string Prefix { get; } = "define";

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            if (txtReader.Current.Remainder.StartsWith(Prefix))
            {
                return Define(txtReader);
            }
            return Resolve(txtReader);
        }

        private (List<string> result, bool invoked) Define(ITextReader txtReader)
        {
            var call = CallParser.GetInvocation(txtReader);

            if (call.name == null)
            {
                return (null, false);
            }

            var args = call.args;

            if (args.Length < 1)
            {
                throw new System.Exception("Invalid def");
            }
            txtReader.Current.Consume(call.length);

            var name = MacroString.Trim(args[0]);

            if (args.Length >= 2)
            {
                var value = args[1];
                _mappings[name] = MacroString.Trim(value);
                if (Regex.IsMatch(value, @"\$\d+") || value.Contains("\"$\""))
                {
                    _hasParameters[name] = true;
                }
                else
                {
                    _hasParameters[name] = false;
                }
            }
            else
            {
                _mappings[name] = string.Empty;
            }
            _regexes[name] = new Regex($@"(?<!`[^']*){Regex.Escape(name)}\b");
            _startRegexes[name] = new Regex($@"^{Regex.Escape(name)}\b");
            return (new List<string> { String.Empty }, true);
        }

        private (List<string> result, bool resolved) Resolve(ITextReader txtReader)
        {
            bool resolved = false;
            string initial = txtReader.Current.Remainder;
            string result = txtReader.Current.Remainder;
            
            foreach (var key in _mappings.Keys)
            {
                var matches = _regexes[key].Matches(txtReader.Current.Remainder);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var replacement = _mappings[key];
                        if (_hasParameters.ContainsKey(key) && _hasParameters[key])
                        {
                            var remainder = result.Substring(match.Index);

                            var call = CallParser.GetInvocation(txtReader, match.Index);

                            replacement = replacement.Replace($"$0", key);

                            int i = 1;

                            if (call.args != null)
                            {
                                if (replacement.Contains("\"$\""))
                                {
                                    replacement = replacement.Replace("\"$\"", "$" + MacroString.Trim(call.args[0]));
                                }

                                foreach (var arg in call.args)
                                {
                                    var argR = MacroString.Trim(call.args[i - 1]);
                                    replacement = replacement.Replace($"${i}", argR);
                                    i++;
                                }
                            }
                            if (call.length > 0)
                            {
                                var callString = remainder.Substring(0, call.length);
                                result = result.Replace(callString, replacement);
                            }
                            else
                            {
                                result = result.Replace(key, replacement);
                            }
                        }
                        else
                        {
                            result = result.Replace(key, replacement);
                        }

                    }
                    resolved = true;
                }
            }

            if (resolved)
            {
                txtReader.Current.Consume(initial.Length);
            }

            return (new List<string>(result.Split(Environment.NewLine)), resolved);
        }

        public void Remove(string name)
        {
            _mappings.Remove(name);
        }

        public bool IsDefined(string name)
        {
            return _mappings.ContainsKey(name);
        }

        public bool CanBeUsed(ITextReader textReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(textReader.Current.Remainder, $"^{Prefix}\b")
                    || _startRegexes.Any(r => r.Value.IsMatch(textReader.Current.Remainder));
            }

            return Regex.IsMatch(textReader.Current.Remainder, @"\s*" + Prefix)
                || _regexes.Any(r => r.Value.IsMatch(textReader.Current.Remainder));
        }
    }
}
