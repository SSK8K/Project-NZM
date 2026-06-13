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
                if(char.IsDigit(symbol))
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

                    // Check if production follows right-linear pattern
                    bool followsRight = false;
                    bool followsLeft = false;

                    // Right-linear: A -> wB یا A -> w (که w شامل یک یا چند terminal است)
                    if (prod.Length >= 1)
                    {
                        // پیدا کردن آخرین کاراکتر
                        char lastChar = prod[prod.Length - 1];

                        if (_grammer.Isterminal(lastChar) && prod.Length == 1)
                        {
                            // A -> a
                            followsRight = true;
                        }
                        else if (_grammer.Isnonterminal(lastChar.ToString()))
                        {
                            // بررسی کنید همه کاراکترهای قبل nonterminal باشند
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
                                // A -> a1 a2 ... an B
                                followsRight = true;
                            }
                        }
                    }

                    // Left-linear: A -> Bw یا A -> w
                    if (prod.Length >= 1)
                    {
                        char firstChar = prod[0];

                        if (_grammer.Isterminal(firstChar) && prod.Length == 1)
                        {
                            // A -> a
                            followsLeft = true;
                        }
                        else if (_grammer.Isnonterminal(firstChar.ToString()))
                        {
                            // بررسی کنید همه کاراکترهای بعد terminal باشند
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
                                // A -> B a1 a2 ... an
                                followsLeft = true;
                            }
                        }
                    }

                    if (!followsRight) isRightRegular = false;
                    if (!followsLeft) isLeftRegular = false;

                    // اگر هر دو false شدند، دیگر نیازی به ادامه نیست
                    if (!isRightRegular && !isLeftRegular)
                        return GrammerType.NotRegular;
                }
            }
            if (isRightRegular && isLeftRegular) return GrammerType.Regular;
            if (isRightRegular) return GrammerType.RightRegular;
            if (isLeftRegular) return GrammerType.LeftRegular;

            return GrammerType.NotRegular;
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
            var dfa = new Dfa();
            dfa.StartState = _grammer.Startsymbol;

            foreach (var nt in _grammer.Nonterminals)
                dfa.Allstates.Add(nt);

            const string finalState = "F";
            const string deadState  = "D";
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
                            Symbol    = sym,
                            ToState   = dead
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
                        Symbol    = sym,
                        ToState   = dead
                    });
                }
            }
        }

        private void AddTransitionSafe(Dfa dfa, string from, char sym, string to)
        {
            bool exists = dfa.Transitions.Any(
                t => t.FromState == from && t.Symbol == sym);
            if (exists)
                throw new Exception(
                    $"Conflict: '{from}' --{sym}--> already exists. " +
                    $"Grammar may be non-deterministic!");

            dfa.Transitions.Add(new DfaTransition
            {
                FromState = from,
                Symbol    = sym,
                ToState   = to
            });
        }
    }
}