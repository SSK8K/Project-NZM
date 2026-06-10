using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNZM
{
   public enum GrammerType
    {
        Regular,RightRegular,LeftRegular,Mixed,NotRegular,unknown
    }
    public class DfaTransition
    {
        public string FromState {get; set;}
        public char Symbol {get; set;}
        public string ToState {get; set;}
    }
    public class DfaTransitionView
    {
        public string Fromstate {get; set;}
        public char Symbol {get; set;}
        public string Tostate{get; set;}
    }
    public class Dfa
    {
        public string StartState {get; set;}
        public HashSet<string> Finalstates {get; set;}
        public HashSet<string> Allstates{get; set;}
        public List<DfaTransition> Transitions {get; set;}
        public Dfa()
        {
            Finalstates = new HashSet<string>();
            Allstates = new HashSet<string>();
            Transitions = new List<DfaTransition>();
        }
        public List<DfaTransitionView>GetTransitionView()
        {
            return Transitions.Select(T =>new DfaTransitionView
            {
                Fromstate = T.Fromstate,
                Symbol = T.Symbol,
                Tostate=T.Tostate
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
            return FinalStates.Contains(currentState);
        }
    }
    public class Grammer
    {
        public string Startsymbol{get; set;}
        public Dictionary<string,List<string>> Productions {get; set;}
        public HashSet<char> Terminals {get; set;}
        public HashSet<string> Nonterminals {get; set;}
        public Grammer()
        {
            Productions = new Dictionary<string, List<string>>();
            Terminals = new HashSet<char>();
            Nonterminals = new HashSet<string>();
        }
        public static Grammer Parse(string[] Lines)
        {
            var grammer = new Grammer();
            if(Lines == null || Lines.Length==0)
            {
                throw new Exception("Input grammer can't be empty");
            }
            var Firstrule = Lines[0];
            var ArrowIndex = Firstrule.IndexOf("->");
            if(ArrowIndex <0 )
            {
                throw new FormatException($"invalid grammer rule format: {Firstrule}");
            }
            grammer.Startsymbol = Firstrule.Substring(0,ArrowIndex).Trim();
            if(string.IsNullOrEmpty(grammer.Startsymbol))
            {
                throw new FormatException("start symbol can't be empty");
            }
            grammer.Nonterminals.Add(grammer.Startsymbol);
            foreach(var line in Lines)
            {
                ArrowIndex = line.IndexOf("->");
                if(ArrowIndex <0)
                {
                    continue;
                }
                string nonterminals = line.Substring(0,ArrowIndex).Trim();
                string ProductionsPart = line.Substring(ArrowIndex+2).Trim();
                if(string.IsNullOrEmpty(nonterminals))
                {
                    throw new FormatException($"Non-terminal is not in rule : {line}");
                }
                if(!grammer.Nonterminals.Contains(nonterminals))
                {
                    grammer.Nonterminals.Add(nonterminals);
                }
                var ProductionList = ProductionsPart.Split('|').Select(P=>P.Trim()).ToList();
                if(!grammer.Productions.ContainsKey(nonterminals))
                {
                    grammer.Productions[nonterminals] = new List<string>();
                }
                grammer.Productions[nonterminals].AddRange(ProductionList);
                foreach (var production in ProductionList)
                {
                    if(production =="ε")
                    {
                        continue;
                    }
                    foreach(char symbol in production)
                    {
                        if(char.IsLower(symbol))
                        {
                            grammer.Terminals.Add(symbol);
                        }
                        else if(char.IsUpper(symbol))
                        {
                            if(!grammer.Nonterminals.Contains(symbol.ToString()))
                            {
                                grammer.Nonterminals.Add(symbol.ToString());
                            }
                        }
                    }
                }
            }
            foreach(var nt in grammer.Nonterminals)
            {
               if(!grammer.Productions.ContainsKey(nt))
                {
                    grammer.Productions[nt] = new List<string>();
                }
            }
            var symbolsInProductions = grammer.Productions.Values.SelectMany(list =>list).SelectMany(p => p.Where(c=>c !='ε'));
            foreach(char symbol in symbolsInProductions)
            {
                if(char.IsLower(symbol))
                {
                    grammer.Terminals.Add(symbol);
                }
                else if(char.IsUpper(symbol))
                {
                    if(!grammer.Nonterminals.Contains(symbol.ToString()))
                    {
                        grammer.Nonterminals.Add(symbol.ToString());
                    }
                }
            }
             if(!grammer.Nonterminals.Contains(grammer.Startsymbol))
                {
                    grammer.Nonterminals.Add(grammer.Startsymbol);
                }
            foreach(var terminal in grammer.Terminals.ToList())
            {
               if(grammer.Nonterminals.Contains(terminal.ToString()))
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

                    // Right-linear: A -> a or A -> aB
                    if (prod.Length == 1 && _grammer.Isterminal(prod[0]))
                        followsRight = true;
                    else if (prod.Length == 2 && 
                            _grammer.Isterminal(prod[0]) && 
                            _grammer.Isnonterminal(prod[1].ToString()))
                        followsRight = true;

                    // Left-linear: A -> a or A -> Ba  
                    if (prod.Length == 1 && _grammer.Isterminal(prod[0]))
                        followsLeft = true;
                    else if (prod.Length == 2 && 
                            _grammer.Isnonterminal(prod[0].ToString()) && 
                            _grammer.Isterminal(prod[1]))
                        followsLeft = true;

                    if (!followsRight) isRightRegular = false;
                    if (!followsLeft) isLeftRegular = false;
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

                        if (prod.Length == 1 && _grammer.Isterminal(prod[0]))
                        {
                            // A -> a
                            dfa.Transitions.Add(new DfaTransition
                            {
                                FromState = nt,
                                Symbol = prod[0],
                                ToState = finalState
                            });
                            dfa.Finalstates.Add(finalState);
                        }
                        else if (prod.Length == 2 && _grammer.Isterminal(prod[0]) && _grammer.Isnonterminal(prod[1].ToString()))
                        {
                            // A -> aB (right regular)
                            dfa.Transitions.Add(new DfaTransition
                            {
                                FromState = nt,
                                Symbol = prod[0],
                                ToState = prod[1].ToString()
                            });
                        }
                        else if (prod.Length == 2 && _grammer.Isnonterminal(prod[0].ToString()) && _grammer.Isterminal(prod[1]))
                        {
                            // A -> Ba (left regular)
                            dfa.Transitions.Add(new DfaTransition
                            {
                                FromState = nt,
                                Symbol = prod[1],
                                ToState = prod[0].ToString()
                            });
                        }
                        else
                        {
                            // This shouldn't happen for regular grammars
                            throw new Exception($"Invalid production for DFA conversion: {nt} -> {prod}");
                        }
                    }
                }

                // Add missing transitions to dead state (complete DFA)
                foreach (var state in dfa.Allstates.ToList())
                {
                    foreach (char symbol in _grammer.Terminals)
                    {
                        bool hasTransition = dfa.Transitions.Any(t => t.FromState == state && t.Symbol == symbol);
                        if (!hasTransition && state != deadState)
                        {
                            dfa.Transitions.Add(new DfaTransition
                            {
                                FromState = state,
                                Symbol = symbol,
                                ToState = deadState
                            });
                        }
                    }
                }

                // Dead state loops on all terminals
                foreach (char symbol in _grammer.Terminals)
                {
                    bool hasDeadTransition = dfa.Transitions.Any(t => t.FromState == deadState && t.Symbol == symbol);
                    if (!hasDeadTransition)
                    {
                        dfa.Transitions.Add(new DfaTransition
                        {
                            FromState = deadState,
                            Symbol = symbol,
                            ToState = deadState
                        });
                    }
                }

                return dfa;
            }
        }
    }