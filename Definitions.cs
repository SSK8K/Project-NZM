using System;


namespace ProjectNZM
{
   public enum GrammerType
    {
        Regular,RightRegular,LeftRegular,Mixed,NotRegular,unknown
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
           // Guard clauses
          if (_grammer == null)
            throw new Exception("Grammar object is null");

         if (_grammer.Productions == null || _grammer.Productions.Count == 0)
          throw new Exception("Grammar has no productions to analyze");

          if (_grammer.Nonterminals == null || _grammer.Nonterminals.Count == 0)
        throw new Exception("Grammar has no non-terminals defined");

         bool hasRightRegular = false;
          bool hasLeftRegular = false;

           for (int i = 0; i < _grammer.Nonterminals.Count; i++)
          {
          string nonTerminal = _grammer.Nonterminals.ElementAt(i);

          if (!_grammer.Productions.ContainsKey(nonTerminal))
            throw new Exception($"Non-terminal '{nonTerminal}' has no production rules defined");

         List<string> productions = _grammer.Productions[nonTerminal];

         if (productions == null || productions.Count == 0)
            throw new Exception($"Non-terminal '{nonTerminal}' has an empty production list");

           for (int j = 0; j < productions.Count; j++)
          {
            string production = productions[j];

            if (string.IsNullOrEmpty(production))
                throw new Exception($"Non-terminal '{nonTerminal}' has an empty or null production");

            bool isRight = CheckRightRegular(production);
            bool isLeft  = CheckLeftRegular(production);

            // Neither right nor left regular → grammar is not regular
            if (!isRight && !isLeft)
                return GrammarType.NotRegular;

            if (isRight) hasRightRegular = true;
            if (isLeft)  hasLeftRegular  = true;
        }
    }

         // Final classification based on observed production types
        if (hasRightRegular && hasLeftRegular) return GrammarType.Mixed;
         if (hasRightRegular)                   return GrammarType.RightRegular;
         if (hasLeftRegular)                    return GrammarType.LeftRegular;

         throw new Exception("Unable to determine grammar type - no valid productions found");
        }
        private bool CheckRightRegular(string production)
         {
        if (string.IsNullOrEmpty(production)) return false;

        // A -> a
         if (production.Length == 1)
        return _grammer.Isterminal(production[0]);

         // A -> aB
         if (production.Length == 2)
        return _grammer.Isterminal(production[0]) &&
               _grammer.Isnonterminal(production[1].ToString());

          return false;
         }

      private bool CheckLeftRegular(string production)
         {
         if (string.IsNullOrEmpty(production)) return false;

         // A -> a
         if (production.Length == 1)
        return _grammer.Isterminal(production[0]);

         // A -> Ba
         if (production.Length == 2)
        return _grammer.Isnonterminal(production[0].ToString()) &&
               _grammer.Isterminal(production[1]);

         return false;
         }
        public Dfa ConverttoDfa()
        {
            //will be completed by sajad
        }
    }
}