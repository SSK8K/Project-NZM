using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNZM
{
    public enum GrammerType
    {
        Regular, RightRegular, LeftRegular, Mixed, NotRegular, unknown
    }
    public class DfaTransition
    {
        public string FromState { get; set; }
        public char Symbol { get; set; }
        public string ToState { get; set; }
    }
    public class DfaTransitionView
    {
        public string Fromstate { get; set; }
        public char Symbol { get; set; }
        public string Tostate { get; set; }
    }
    public class Dfa
    {
        public string StartState { get; set; }
        public HashSet<string> Finalstates { get; set; }
        public HashSet<string> Allstates { get; set; }
        public List<DfaTransition> Transitions { get; set; }
        public Dfa()
        {
            Finalstates = new HashSet<string>();
            Allstates = new HashSet<string>();
            Transitions = new List<DfaTransition>();
        }
        public List<DfaTransitionView> GetTransitionView()
        {
            return Transitions.Select(T => new DfaTransitionView
            {
                Fromstate = T.FromState,
                Symbol = T.Symbol,
                Tostate = T.ToState
            }).ToList();
        }
        public bool Accepts(string input)
        {
            string currentState = StartState;
            foreach (char symbol in input)
            {
                var transition = Transitions.FirstOrDefault(t => t.FromState == currentState && t.Symbol == symbol);
                if (transition == null)
                {
                    return false;
                }
                currentState = transition.ToState;
            }
            return Finalstates.Contains(currentState);
        }
    }
    public class Grammer
    {
        public string Startsymbol { get; set; }
        public Dictionary<string, List<string>> Productions { get; set; }
        public HashSet<char> Terminals { get; set; }
        public HashSet<string> Nonterminals { get; set; }
        public Grammer()
        {
            Productions = new Dictionary<string, List<string>>();
            Terminals = new HashSet<char>();
            Nonterminals = new HashSet<string>();
        }
        public static Grammer Parse(string[] Lines)
        {
            var grammer = new Grammer();
            if (Lines == null || Lines.Length == 0)
            {
                throw new Exception("Input grammer can't be empty");
            }
            var Firstrule = Lines[0];
            var ArrowIndex = Firstrule.IndexOf("->");
            if (ArrowIndex < 0)
            {
                throw new FormatException($"invalid grammer rule format: {Firstrule}");
            }
            grammer.Startsymbol = Firstrule.Substring(0, ArrowIndex).Trim();
            if (string.IsNullOrEmpty(grammer.Startsymbol))
            {
                throw new FormatException("start symbol can't be empty");
            }
            grammer.Nonterminals.Add(grammer.Startsymbol);
            foreach (var line in Lines)
            {
                ArrowIndex = line.IndexOf("->");
                if (ArrowIndex < 0)
                {
                    continue;
                }
                string nonterminals = line.Substring(0, ArrowIndex).Trim();
                string ProductionsPart = line.Substring(ArrowIndex + 2).Trim();
                if (string.IsNullOrEmpty(nonterminals))
                {
                    throw new FormatException($"Non-terminal is not in rule : {line}");
                }
                if (!grammer.Nonterminals.Contains(nonterminals))
                {
                    grammer.Nonterminals.Add(nonterminals);
                }
                var ProductionList = ProductionsPart.Split('|').Select(P => P.Trim()).ToList();
                if (!grammer.Productions.ContainsKey(nonterminals))
                {
                    grammer.Productions[nonterminals] = new List<string>();
                }
                grammer.Productions[nonterminals].AddRange(ProductionList);
                foreach (var production in ProductionList)
                {
                    if (production == "ε" || production == "λ")
                    {
                        continue;
                    }
                    foreach (char symbol in production)
                    {
                        if (char.IsLower(symbol))
                        {
                            grammer.Terminals.Add(symbol);
                        }
                        else if (char.IsUpper(symbol))
                        {
                            if (!grammer.Nonterminals.Contains(symbol.ToString()))
                            {
                                grammer.Nonterminals.Add(symbol.ToString());
                            }
                        }
                    }
                }
            }
            foreach (var nt in grammer.Nonterminals)
            {
                if (!grammer.Productions.ContainsKey(nt))
                {
                    grammer.Productions[nt] = new List<string>();
                }
            }
            var symbolsInProductions = grammer.Productions.Values.SelectMany(list => list).SelectMany(p => p.Where(c => c != 'ε' && c != 'λ'));
            foreach (char symbol in symbolsInProductions)
            {
                if (char.IsDigit(symbol))
                {
                    grammer.Terminals.Add(symbol);
                }
                else if (char.IsLower(symbol))
                {
                    grammer.Terminals.Add(symbol);
                }
                else if (char.IsUpper(symbol))
                {
                    if (!grammer.Nonterminals.Contains(symbol.ToString()))
                    {
                        grammer.Nonterminals.Add(symbol.ToString());
                    }
                }
            }
            if (!grammer.Nonterminals.Contains(grammer.Startsymbol))
            {
                grammer.Nonterminals.Add(grammer.Startsymbol);
            }
            foreach (var terminal in grammer.Terminals.ToList())
            {
                if (grammer.Nonterminals.Contains(terminal.ToString()))
                {
                    grammer.Nonterminals.Remove(terminal.ToString());
                }
            }
            return grammer;
        }
        public bool Isterminal(char symbol)
        {
            return Terminals.Contains(symbol);
        }
        public bool Isnonterminal(string symbol)
        {
            return Nonterminals.Contains(symbol);
        }
    }
    public class GrammarConverter
    {
        readonly Grammer _grammer;
        public GrammarConverter(Grammer grammer)
        {
            _grammer = grammer ?? throw new Exception(nameof(grammer));
        }
        public GrammerType DetermineType()
        {
            bool isRightRegular = true;
            bool isLeftRegular = true;

            foreach (var nt in _grammer.Nonterminals)
            {
                if (!_grammer.Productions.ContainsKey(nt)) continue;

                foreach (var prod in _grammer.Productions[nt])
                {
                    if (prod == "ε" || prod == "λ") continue;

                    bool followsRight = false;
                    bool followsLeft = false;

                    if (prod.Length >= 1)
                    {
                        char lastChar = prod[prod.Length - 1];

                        if (_grammer.Isterminal(lastChar) && prod.Length == 1)
                        {
                            followsRight = true;
                        }
                        else if (_grammer.Isnonterminal(lastChar.ToString()))
                        {
                            bool allBeforeAreTerminals = true;
                            for (int i = 0; i < prod.Length - 1; i++)
                            {
                                if (!_grammer.Isterminal(prod[i]))
                                {
                                    allBeforeAreTerminals = false;
                                    break;
                                }
                            }
                            if (allBeforeAreTerminals)
                            {
                                followsRight = true;
                            }
                        }
                    }

                    if (prod.Length >= 1)
                    {
                        char firstChar = prod[0];

                        if (_grammer.Isterminal(firstChar) && prod.Length == 1)
                        {
                            followsLeft = true;
                        }
                        else if (_grammer.Isnonterminal(firstChar.ToString()))
                        {
                            bool allAfterAreTerminals = true;
                            for (int i = 1; i < prod.Length; i++)
                            {
                                if (!_grammer.Isterminal(prod[i]))
                                {
                                    allAfterAreTerminals = false;
                                    break;
                                }
                            }
                            if (allAfterAreTerminals)
                            {
                                followsLeft = true;
                            }
                        }
                    }

                    if (!followsRight) isRightRegular = false;
                    if (!followsLeft) isLeftRegular = false;

                    if (!isRightRegular && !isLeftRegular)
                        return GrammerType.NotRegular;
                }
            }
            if (isRightRegular && isLeftRegular) return GrammerType.Regular;
            if (isRightRegular) return GrammerType.RightRegular;
            if (isLeftRegular) return GrammerType.LeftRegular;

            return GrammerType.NotRegular;
        }
        public bool IsNfaLike()
        {
            var type = DetermineType();
            var seen = new Dictionary<(string, char), string>();

            foreach (var nt in _grammer.Nonterminals)
            {
                if (!_grammer.Productions.ContainsKey(nt)) continue;

                foreach (var prod in _grammer.Productions[nt])
                {
                    if (prod == "ε" || prod == "λ") continue;

                    string destination;
                    char triggerChar;

                    if (type == GrammerType.LeftRegular)
                    {
                        char lastChar = prod[prod.Length - 1];
                        if (!_grammer.Isterminal(lastChar)) continue;

                        triggerChar = lastChar;
                        destination = prod.Length == 1
                            ? "F"
                            : prod[0].ToString();
                    }
                    else
                    {
                        char firstChar = prod[0];
                        if (!_grammer.Isterminal(firstChar)) continue;

                        triggerChar = firstChar;
                        destination = prod.Length == 1
                            ? "F"
                            : prod[prod.Length - 1].ToString();
                    }

                    var key = (nt, triggerChar);
                    if (seen.ContainsKey(key) && seen[key] != destination)
                        return true;

                    seen[key] = destination;
                }
            }
            return false;
        }

        private Dictionary<(string, char), HashSet<string>> BuildNfaTransitions(
            GrammerType type, out HashSet<string> nfaFinals)
        {
            var nfaTrans = new Dictionary<(string, char), HashSet<string>>();
            nfaFinals = new HashSet<string>();
            const string finalState = "F";

            foreach (var nt in _grammer.Nonterminals)
            {
                if (!_grammer.Productions.ContainsKey(nt)) continue;

                foreach (var prod in _grammer.Productions[nt])
                {
                    if (prod == "ε" || prod == "λ")
                    {
                        nfaFinals.Add(nt);
                        continue;
                    }

                    if (type == GrammerType.LeftRegular)
                    {
                        char firstChar = prod[0];

                        if (_grammer.Isnonterminal(firstChar.ToString()))
                        {
                            string targetNT = firstChar.ToString();
                            string w = prod.Substring(1);
                            string reversed = new string(w.Reverse().ToArray());

                            if (reversed.Length == 0) continue;

                            string cur = nt;
                            for (int i = 0; i < reversed.Length; i++)
                            {
                                string next = (i == reversed.Length - 1)
                                    ? targetNT
                                    : $"q_{nt}_{targetNT}_{i}";

                                var key = (cur, reversed[i]);
                                if (!nfaTrans.ContainsKey(key))
                                    nfaTrans[key] = new HashSet<string>();
                                nfaTrans[key].Add(next);
                                cur = next;
                            }
                        }
                        else
                        {
                            nfaFinals.Add(finalState);
                            string cur = nt;
                            string reversedProd = new string(prod.Reverse().ToArray());
                            for (int i = 0; i < reversedProd.Length; i++)
                            {
                                string next = (i == reversedProd.Length - 1)
                                    ? finalState
                                    : $"q_{nt}_t_{i}";

                                var key = (cur, reversedProd[i]);
                                if (!nfaTrans.ContainsKey(key))
                                    nfaTrans[key] = new HashSet<string>();
                                nfaTrans[key].Add(next);
                                cur = next;
                            }
                        }
                    }
                    else
                    {
                        char lastChar = prod[prod.Length - 1];

                        if (_grammer.Isterminal(lastChar))
                        {
                            nfaFinals.Add(finalState);
                            string cur = nt;
                            for (int i = 0; i < prod.Length; i++)
                            {
                                string next = (i == prod.Length - 1)
                                    ? finalState
                                    : $"q_{nt}_r_{i}";

                                var key = (cur, prod[i]);
                                if (!nfaTrans.ContainsKey(key))
                                    nfaTrans[key] = new HashSet<string>();
                                nfaTrans[key].Add(next);
                                cur = next;
                            }
                        }
                        else if (_grammer.Isnonterminal(lastChar.ToString()))
                        {
                            string cur = nt;
                            for (int i = 0; i < prod.Length - 1; i++)
                            {
                                string next = (i == prod.Length - 2)
                                    ? lastChar.ToString()
                                    : $"q_{nt}_r_{i}";

                                var key = (cur, prod[i]);
                                if (!nfaTrans.ContainsKey(key))
                                    nfaTrans[key] = new HashSet<string>();
                                nfaTrans[key].Add(next);
                                cur = next;
                            }
                        }
                    }
                }
            }
            return nfaTrans;
        }

        private Dfa SubsetConstruction(
            string nfaStart,
            Dictionary<(string, char), HashSet<string>> nfaTrans,
            HashSet<string> nfaFinals)
        {
            var dfa = new Dfa();
            const string deadState = "D";

            var startSet = new HashSet<string> { nfaStart };
            string startName = SetName(startSet);
            dfa.StartState = startName;

            var worklist = new Queue<HashSet<string>>();
            var visited = new Dictionary<string, HashSet<string>>();

            worklist.Enqueue(startSet);
            visited[startName] = startSet;
            dfa.Allstates.Add(startName);

            if (startSet.Any(s => nfaFinals.Contains(s)))
                dfa.Finalstates.Add(startName);

            while (worklist.Count > 0)
            {
                var current = worklist.Dequeue();
                string currentName = SetName(current);

                foreach (char sym in _grammer.Terminals)
                {
                    var nextSet = new HashSet<string>();
                    foreach (var state in current)
                    {
                        var key = (state, sym);
                        if (nfaTrans.ContainsKey(key))
                            foreach (var s in nfaTrans[key])
                                nextSet.Add(s);
                    }

                    string nextName = nextSet.Count == 0 ? deadState : SetName(nextSet);

                    dfa.Transitions.Add(new DfaTransition
                    {
                        FromState = currentName,
                        Symbol = sym,
                        ToState = nextName
                    });

                    if (nextSet.Count == 0)
                    {
                        dfa.Allstates.Add(deadState);
                        continue;
                    }

                    if (!visited.ContainsKey(nextName))
                    {
                        visited[nextName] = nextSet;
                        dfa.Allstates.Add(nextName);
                        worklist.Enqueue(nextSet);

                        if (nextSet.Any(s => nfaFinals.Contains(s)))
                            dfa.Finalstates.Add(nextName);
                    }
                }
            }

            if (dfa.Allstates.Contains(deadState))
            {
                foreach (char sym in _grammer.Terminals)
                {
                    bool has = dfa.Transitions.Any(
                        t => t.FromState == deadState && t.Symbol == sym);
                    if (!has)
                        dfa.Transitions.Add(new DfaTransition
                        {
                            FromState = deadState,
                            Symbol = sym,
                            ToState = deadState
                        });
                }
            }

            return dfa;
        }

        private string SetName(HashSet<string> set)
        {
            return "{" + string.Join(",", set.OrderBy(s => s)) + "}";
        }
        public Dfa ConvertToDFA()
        {
            var type = DetermineType();
            return (type == GrammerType.LeftRegular)
                ? ConvertLeftLinearToDFA()
                : ConvertRightLinearToDFA();
        }
        private Dfa ConvertRightLinearToDFA()
        {
            if (IsNfaLike())
            {
                var nfaTrans = BuildNfaTransitions(GrammerType.RightRegular, out var nfaFinals);
                return SubsetConstruction(_grammer.Startsymbol, nfaTrans, nfaFinals);
            }
            var dfa = new Dfa();
            dfa.StartState = _grammer.Startsymbol;

            foreach (var nt in _grammer.Nonterminals)
                dfa.Allstates.Add(nt);

            const string finalState = "F";
            const string deadState = "D";
            dfa.Allstates.Add(finalState);
            dfa.Allstates.Add(deadState);

            foreach (var nt in _grammer.Nonterminals)
            {
                if (!_grammer.Productions.ContainsKey(nt)) continue;

                foreach (var prod in _grammer.Productions[nt])
                {
                    if (prod == "ε" || prod == "λ")
                    {
                        dfa.Finalstates.Add(nt);
                        continue;
                    }

                    char lastChar = prod[prod.Length - 1];

                    if (_grammer.Isterminal(lastChar))
                    {
                        bool allTerminals = prod.All(c => _grammer.Isterminal(c));
                        if (!allTerminals)
                            throw new Exception($"Invalid production: {nt} -> {prod}");

                        string cur = nt;
                        for (int i = 0; i < prod.Length; i++)
                        {
                            string next;
                            if (i == prod.Length - 1)
                            {
                                next = finalState;
                            }
                            else
                            {
                                next = $"q_{nt}_r_{i}";
                                if (!dfa.Allstates.Contains(next))
                                    dfa.Allstates.Add(next);
                            }
                            AddTransitionSafe(dfa, cur, prod[i], next);
                            cur = next;
                        }
                        dfa.Finalstates.Add(finalState);
                    }
                    else if (_grammer.Isnonterminal(lastChar.ToString()))
                    {
                        bool allBeforeTerminals = prod.Take(prod.Length - 1)
                                                      .All(c => _grammer.Isterminal(c));
                        if (!allBeforeTerminals)
                            throw new Exception($"Invalid production: {nt} -> {prod}");

                        string cur = nt;
                        for (int i = 0; i < prod.Length - 1; i++)
                        {
                            string next = (i == prod.Length - 2)
                                ? lastChar.ToString()
                                : $"q_{nt}_r_{i}";

                            if (!dfa.Allstates.Contains(next))
                                dfa.Allstates.Add(next);

                            AddTransitionSafe(dfa, cur, prod[i], next);
                            cur = next;
                        }
                    }
                }
            }

            FillDeadTransitions(dfa, deadState);
            return dfa;
        }
        private Dfa ConvertLeftLinearToDFA()
        {
            if (IsNfaLike())
            {
                var nfaTrans = BuildNfaTransitions(GrammerType.LeftRegular, out var nfaFinals);
                return SubsetConstruction(_grammer.Startsymbol, nfaTrans, nfaFinals);
            }
            var dfa = new Dfa();

            foreach (var nt in _grammer.Nonterminals)
                dfa.Allstates.Add(nt);

            const string deadState = "D";
            dfa.Allstates.Add(deadState);
            dfa.StartState = _grammer.Startsymbol;

            foreach (var kvp in _grammer.Productions)
                if (kvp.Value.Any(p => p == "ε" || p == "λ"))
                    dfa.Finalstates.Add(kvp.Key);

            foreach (var kvp in _grammer.Productions)
            {
                string lhs = kvp.Key;

                foreach (var prod in kvp.Value)
                {
                    if (prod == "ε" || prod == "λ") continue;

                    char firstChar = prod[0];

                    if (_grammer.Isnonterminal(firstChar.ToString()))
                    {
                        string targetNT = firstChar.ToString();
                        string terminals = new string(prod.Substring(1).Reverse().ToArray());   // w

                        if (terminals.Length == 0) continue;

                        if (terminals.Length == 1)
                        {
                            AddTransitionSafe(dfa, lhs, terminals[0], targetNT);
                        }
                        else
                        {
                            string cur = lhs;
                            for (int i = 0; i < terminals.Length; i++)
                            {
                                string next;
                                if (i == terminals.Length - 1)
                                {
                                    next = targetNT;
                                }
                                else
                                {
                                    next = $"q_{lhs}_{targetNT}_{i}";
                                    if (!dfa.Allstates.Contains(next))
                                        dfa.Allstates.Add(next);
                                }
                                AddTransitionSafe(dfa, cur, terminals[i], next);
                                cur = next;
                            }
                        }
                    }
                    else
                    {
                        const string pureTerminalFinal = "F";
                        if (!dfa.Allstates.Contains(pureTerminalFinal))
                            dfa.Allstates.Add(pureTerminalFinal);
                        if (!dfa.Finalstates.Contains(pureTerminalFinal))
                            dfa.Finalstates.Add(pureTerminalFinal);

                        if (prod.Length == 1)
                        {
                            AddTransitionSafe(dfa, lhs, prod[0], pureTerminalFinal);
                        }
                        else
                        {
                            string cur = lhs;
                            for (int i = 0; i < prod.Length; i++)
                            {
                                string next;
                                if (i == prod.Length - 1)
                                {
                                    next = pureTerminalFinal;
                                }
                                else
                                {
                                    next = $"q_{lhs}_t_{i}";
                                    if (!dfa.Allstates.Contains(next))
                                        dfa.Allstates.Add(next);
                                }
                                AddTransitionSafe(dfa, cur, prod[i], next);
                                cur = next;
                            }
                        }
                    }
                }
            }

            FillDeadTransitions(dfa, deadState);
            return dfa;
        }
        private void FillDeadTransitions(Dfa dfa, string dead)
        {
            var allStatesSnapshot = dfa.Allstates.ToList();

            foreach (var st in allStatesSnapshot)
            {
                foreach (char sym in _grammer.Terminals)
                {
                    bool has = dfa.Transitions.Any(
                        t => t.FromState == st && t.Symbol == sym);
                    if (!has && st != dead)
                    {
                        dfa.Transitions.Add(new DfaTransition
                        {
                            FromState = st,
                            Symbol = sym,
                            ToState = dead
                        });
                    }
                }
            }
            if (!dfa.Allstates.Contains(dead))
                dfa.Allstates.Add(dead);

            foreach (char sym in _grammer.Terminals)
            {
                bool has = dfa.Transitions.Any(
                    t => t.FromState == dead && t.Symbol == sym);
                if (!has)
                {
                    dfa.Transitions.Add(new DfaTransition
                    {
                        FromState = dead,
                        Symbol = sym,
                        ToState = dead
                    });
                }
            }
        }

        private void AddTransitionSafe(Dfa dfa, string from, char sym, string to)
        {


            dfa.Transitions.Add(new DfaTransition
            {
                FromState = from,
                Symbol = sym,
                ToState = to
            });
        }
    }
}