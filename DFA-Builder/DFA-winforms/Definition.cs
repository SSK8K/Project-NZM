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
            var dfa = new Dfa();
            dfa.StartState = _grammer.Startsymbol;

            // Add all nonterminals as states
            foreach (var nt in _grammer.Nonterminals)
                dfa.Allstates.Add(nt);

            string finalState = "F";
            string deadState = "D";

            dfa.Allstates.Add(finalState);
            dfa.Allstates.Add(deadState);

            // First pass: create transitions from grammar rules
            foreach (var nt in _grammer.Nonterminals)
            {
                if (!_grammer.Productions.ContainsKey(nt)) continue;

                foreach (var prod in _grammer.Productions[nt])
                {
                    if (prod == "ε" || prod == "λ")
                    {
                        // ε-production: nt is a final state
                        dfa.Finalstates.Add(nt);
                        continue;
                    }

                    if (prod.Length >= 1)
                    {
                        char lastChar = prod[prod.Length - 1];

                        if (_grammer.Isterminal(lastChar) && prod.Length == 1)
                        {
                            // A -> a
                            AddTransitionWithConflictCheck(dfa, nt, prod[0], finalState);
                            dfa.Finalstates.Add(finalState);
                        }
                        else if (_grammer.Isnonterminal(lastChar.ToString()))
                        {
                            // بررسی کنید بقیه terminal هستند
                            bool allTerminals = true;
                            for (int i = 0; i < prod.Length - 1; i++)
                            {
                                if (!_grammer.Isterminal(prod[i]))
                                {
                                    allTerminals = false;
                                    break;
                                }
                            }

                            if (allTerminals)
                            {
                                string currentState = nt;
                                // ایجاد state‌های میانی برای زنجیره terminal‌ها
                                for (int i = 0; i < prod.Length - 1; i++)
                                {
                                    string nextState;
                                    if (i == prod.Length - 2)
                                    {
                                        // آخرین transition به nonterminal اصلی
                                        nextState = lastChar.ToString();
                                    }
                                    else
                                    {
                                        // state موقت
                                        nextState = $"q_{nt}_{i}";
                                        if (!dfa.Allstates.Contains(nextState))
                                            dfa.Allstates.Add(nextState);
                                    }

                                    AddTransitionWithConflictCheck(dfa, currentState, prod[i], nextState);
                                    currentState = nextState;
                                }
                            }
                            else
                            {
                                throw new Exception($"Invalid production for DFA conversion: {nt} -> {prod}");
                            }
                        }
                    }
                }
            }

            // اضافه کردن transitions به dead state
            foreach (var state in dfa.Allstates.ToList())
            {
                foreach (char symbol in _grammer.Terminals)
                {
                    bool hasTransition = dfa.Transitions.Any(t => t.FromState == state && t.Symbol == symbol);
                    if (!hasTransition && state != deadState)
                    {
                        AddTransitionWithConflictCheck(dfa, state, symbol, deadState);
                    }
                }
            }

            foreach (char symbol in _grammer.Terminals)
            {
                bool hasDeadTransition = dfa.Transitions.Any(t => t.FromState == deadState && t.Symbol == symbol);
                if (!hasDeadTransition)
                {
                    AddTransitionWithConflictCheck(dfa, deadState, symbol, deadState);
                }
            }

            return dfa;
        }

        // متد کمکی برای چک کردن conflict
        private void AddTransitionWithConflictCheck(Dfa dfa, string fromState, char symbol, string toState)
        {
            bool exists = dfa.Transitions.Any(t => t.FromState == fromState && t.Symbol == symbol);
            if (exists)
            {
                throw new Exception($"Conflict detected: From state '{fromState}' with symbol '{symbol}' already has a transition. Grammar is non-deterministic!");
            }

            dfa.Transitions.Add(new DfaTransition
            {
                FromState = fromState,
                Symbol = symbol,
                ToState = toState
            });
        }
    }
}