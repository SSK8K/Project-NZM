# Project-NZM
This is a repository ready for the Theory of Languages and Automata project
## how to setup the second remtoe?
1. add remote:
   1. enter `git remote add gitlab https://gitlab.aranserver.com/SajadSK/project-nzm.git`
   2. enter `git remote -v` to see remote URLs.
2. configure push command:
   1. enter `git remote set-url --add origin https://gitlab.aranserver.com/SajadSK/project-nzm.git`
   2. enter `git remote get-url --all origin`
   3. You should see two URLs
- It pushes to **both GitHub and GitLab automatically**

## The goals of this project

This is a program to understad whether a given grammar of a language is regular or not. Then if it is regular, It will create and show the DFA of the language graphically.