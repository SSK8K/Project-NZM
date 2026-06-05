using System;


namespace ProjectNZM
{
   public enum GrammarType
    {
        Regular,RightRegular,LeftRegular,Mixed,Irregular,unknown
    }
    public class DFATransition
    {
        public string fromState {get; set;}
        public char symbol {get; set;}
        public string toState{get; set;}
    }
    public class DFA
    {
        public string startState {get; set;}
        public HashSet<string> finalState {get; set;}
        public HashSet<string> allStates {get; set;}
        public List<DFATransition> transitions {get; set;}
        public DFA()
        {
            finalState = new HashSet<string>();
            allStates = new HashSet<string>();
            transitions = new List<DFATransition>();
        }
        public List<DFATransition>GetTransitionView()
        {
            return transitions.Select(terminals =>new DFATransition
            {
                fromState = terminals.fromState,
                symbol = terminals.symbol,
                toState=terminals.toState
            }).ToList();
        }
        public bool Accepts(string input)
        {
           string currentState = startState;
           foreach (char symbol in input)
             {
               var transition = Transitions.FirstOrDefault(t => t.FromState == currentState && t.symbol == symbol);
               if (transition == null)
                 {
                    return false; 
                 }
               currentState = transition.ToState;
             }
            return finalState.Contains(currentState);
        }
    }
    public class Grammar
    {
        public string startSymbol{get; set;}
        public Dictionary<string,List<string>> productions {get; set;}
        public HashSet<char> terminals {get; set;}
        public HashSet<string> variables {get; set;}
        public Grammar()
        {
            productions = new Dictionary<string, List<string>>();
            terminals = new HashSet<char>();
            variables = new HashSet<string>();
        }
        public static Grammar Parse(string[] Lines)
        {
            var grammar = new Grammar();
            if(Lines == null || Lines.Length==0)
            {
                throw new Exception("Input grammar can't be empty");
            }
            var firstRule = Lines[0];
            var arrowIndex = firstRule.IndexOf("->");
            if(arrowIndex <0 )
            {
                throw new FormatException($"invalid grammar rule format: {firstRule}");
            }
            grammar.startSymbol = firstRule.Substring(0,arrowIndex).Trim();
            if(string.IsNullOrEmpty(grammar.startSymbol))
            {
                throw new FormatException("start symbol can't be empty");
            }
            grammar.variables.Add(grammar.startSymbol);
            foreach(var line in Lines)
            {
                arrowIndex = line.IndexOf("->");
                if(arrowIndex <0)
                {
                    continue;
                }
                var nonTerminals = line.Substring(0,arrowIndex).Trim();
                var productionsPart = line.Substring(arrowIndex+2).Trim();
                if(string.IsNullOrEmpty(nonTerminals))
                {
                    throw new FormatException($"Non-terminal is not in rule : {line}");
                }
                if(!grammar.variables.Contains(nonTerminals))
                {
                    grammar.variables.Add(nonTerminals);
                }
                var productionList = productionsPart.Split('|').Select(productions=>productions.Trim()).ToList();
                if(!grammar.productions.ContainsKey(nonTerminals))
                {
                    grammar.productions[nonTerminals] = new List<string>();
                }
                grammar.productions[nonTerminals].AddRange(productionList);
                foreach (var production in productionList)
                {
                    if(production =="ε")
                    {
                        continue;
                    }
                    foreach(char symbol in production)
                    {
                        if(char.IsLower(symbol))
                        {
                            grammar.terminals.Add(symbol);
                        }
                        else if(char.IsUpper(symbol))
                        {
                            if(!grammar.variables.Contains(symbol.ToString()))
                            {
                                grammar.variables.Add(symbol.ToString());
                            }
                        }
                    }
                }
            }
            foreach(var nt in grammar.variables)
            {
               if(!grammar.productions.ContainsKey(nt))
                {
                    grammar.productions[nt] = new List<string>();
                }
            }
            var symbolsInProductions = grammar.productions.Values.SelectMany(list =>list).SelectMany(p => p.Where(c=>c !='ε'));
            foreach(char symbol in symbolsInProductions)
            {
                if(char.IsLower(symbol))
                {
                    grammar.terminals.Add(symbol);
                }
                else if(char.IsUpper(symbol))
                {
                    if(!grammar.variables.Contains(symbol.ToString()))
                    {
                        grammar.variables.Add(symbol.ToString());
                    }
                }
            }
             if(!grammar.variables.Contains(grammar.startSymbol))
                {
                    grammar.variables.Add(grammar.startSymbol);
                }
            foreach(var terminal in grammar.terminals.ToList())
            {
               if(grammar.variables.Contains(terminal.ToString()))
                {
                    grammar.variables.Remove(terminal.ToString());
                } 
            }
            return grammar;
        }
        public bool IsTerminal(char symbol)
        {
            return terminals.Contains(symbol);
        }
        public bool IsNonTerminal(string symbol)
        {
            return variables.Contains(symbol);
        }
    }
    public class GrammarConverter
    {
        readonly Grammar _grammar;
        public GrammarConverter(Grammar grammar)
        {
            _grammar = grammar ?? throw new Exception(nameof(grammar));
        }
        public GrammarType DetermineType()
        {
            // Guard clauses
            if (_grammar == null)
                throw new Exception("Grammar object is null");

            if (_grammar.productions == null || _grammar.productions.Count == 0)
                throw new Exception("Grammar has no productions to analyze");

            if (_grammar.variables == null || _grammar.variables.Count == 0)
                throw new Exception("Grammar has no non-terminals defined");

            bool hasRightRegular = false;
            bool hasLeftRegular = false;

            for (int i = 0; i < _grammar.variables.Count; i++)
            {
                string nonTerminal = _grammar.variables.ElementAt(i);

                if (!_grammar.productions.ContainsKey(nonTerminal))
                    continue;

                List<string> productions = _grammar.productions[nonTerminal];

                if (productions == null || productions.Count == 0)
                    continue;

                for (int j = 0; j < productions.Count; j++)
                {
                    string production = productions[j];

                    if (string.IsNullOrEmpty(production))
                        throw new Exception($"Non-terminal '{nonTerminal}' has an empty or null production");

                    if (production == "ε" || production == "λ")
                      {
                         hasRightRegular = true;
                         continue;
                      }
                    bool isRight = CheckRightRegular(production);
                    bool isLeft = CheckLeftRegular(production);

                    // Neither right nor left regular → grammar is not regular
                    if (!isRight && !isLeft)
                        return GrammarType.Irregular;

                    if (isRight) hasRightRegular = true;
                    if (isLeft) hasLeftRegular = true;
                }
            }

            // Final classification based on observed production types
            if (hasRightRegular && hasLeftRegular) return GrammarType.Mixed;
            if (hasRightRegular) return GrammarType.RightRegular;
            if (hasLeftRegular) return GrammarType.LeftRegular;

            throw new Exception("Unable to determine grammar type - no valid productions found");
        }
        private bool CheckRightRegular(string production)
        {
            if (string.IsNullOrEmpty(production)) return false;

            // A -> a
            if (production.Length == 1)
                return _grammar.Isterminal(production[0]);

            // A -> aB
            if (production.Length == 2)
                return _grammar.Isterminal(production[0]) &&
                       _grammar.Isnonterminal(production[1].ToString());

            return false;
        }

        private bool CheckLeftRegular(string production)
        {
            if (string.IsNullOrEmpty(production)) return false;


            // A -> Ba only
            if (production.Length == 2)
                return _grammar.Isnonterminal(production[0].ToString()) &&
                       _grammar.Isterminal(production[1]);

            return false;
        }
        public DFA ConveretToDFA()
        {
            //will be completed by sajad
        }
    }
}