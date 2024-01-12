using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Poker
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);
        static uint STD_OUTPUT_HANDLE = 0xfffffff5;
        static IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

        public static bool fin = false;

        public enum couleur { VERT = 10, ROUGE = 12, JAUNE = 14, BLANC = 15, NOIRE = 0, ROUGESURBLANC = 252, NOIRESURBLANC = 240 };

        public struct coordonnees
        {
            public int x;
            public int y;
        }

        public struct carte
        {
            public char valeur;
            public int famille;
        }

        public struct STOCK
        {
            public char valeur;
            public int famille;
        }

        public enum combinaison { RIEN, PAIRE, DOUBLE_PAIRE, BRELAN, QUINTE, FULL, COULEUR, CARRE, QUINTE_FLUSH };

        public static char[] valeurs = { 'A', 'R', 'D', 'V', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        public static int[] familles = { 3, 4, 5, 6 };
        public static STOCK[] stock = new STOCK[5];

        public static int[] echange = { 0, 0, 0, 0 };
        public static carte[] MonJeu = new carte[5];

        static Random random = new Random();

        public static carte Tirage()
        {
            carte nouvelleCarte;
            nouvelleCarte.valeur = valeurs[random.Next(valeurs.Length)];
            nouvelleCarte.famille = familles[random.Next(familles.Length)];
            return nouvelleCarte;
        }

        public static bool CarteUnique(carte uneCarte, carte[] unJeu, int numero)
        { 
        
    // Vérifier si la carte n'a pas été choisie
    for (int i = 0; i < unJeu.Length; i++)
    {
        if (i != numero && unJeu[i].valeur == uneCarte.valeur && unJeu[i].famille == uneCarte.famille)
        {
            return false;
            break;
        }
        
   
    } return true;}
    


        public static combinaison ChercheCombinaison(ref carte[] unJeu)
        {
             // Vérifier s'il y a une paire
 for (int i = 0; i < unJeu.Length - 1; i++)
 {
     for (int j = i + 1; j < unJeu.Length; j++)
     {
         if (unJeu[i].valeur == unJeu[j].valeur)
         {
             return combinaison.PAIRE;
         }
     }
 }

 // Vérifier s'il y a une double paire
 int nbPaires = 0;
 for (int i = 0; i < unJeu.Length - 1; i++)
 {
     for (int j = i + 1; j < unJeu.Length; j++)
     {
         if (unJeu[i].valeur == unJeu[j].valeur)
         {
             nbPaires++;
             if (nbPaires == 2)
             {
                 return combinaison.DOUBLE_PAIRE;
             }
         }
     }
 }

 // Vérifier s'il y a un brelan
 for (int i = 0; i < unJeu.Length - 2; i++)
 {
     for (int j = i + 1; j < unJeu.Length - 1; j++)
     {
         for (int k = j + 1; k < unJeu.Length; k++)
         {
             if (unJeu[i].valeur == unJeu[j].valeur && unJeu[i].valeur == unJeu[k].valeur)
             {
                 return combinaison.BRELAN;
             }
         }
     }
 }

 // Vérifier s'il y a une quinte (straight)
 for (int i = 0; i < unJeu.Length - 1; i++)
 {
     int consecutiveCount = 1;
     for (int j = i + 1; j < unJeu.Length; j++)
     {
         if (unJeu[j].valeur - unJeu[i].valeur == consecutiveCount)
         {
             consecutiveCount++;
             if (consecutiveCount == 5)
             {
                 return combinaison.QUINTE;
             }
         }
     }
 }
 // Vérifier s'il y a une couleur
 bool hasCouleur = true;
 for (int i = 0; i < unJeu.Length - 1; i++)
 {
     if (unJeu[i].famille != unJeu[i + 1].famille)
     {
         hasCouleur = false;
         break;
     }
 }

 if (hasCouleur)
 {
     return combinaison.COULEUR;
 }

 // Vérifier s'il y a un full
 bool hasBrelan = false;
 bool hasPaire = false;

 for (int i = 0; i < unJeu.Length - 2; i++)
 {
     if (unJeu[i].valeur == unJeu[i + 1].valeur && unJeu[i + 1].valeur == unJeu[i + 2].valeur)
     {
         hasBrelan = true;

         // Exclure ces cartes pour ne pas les compter comme paire
         unJeu[i] = unJeu[i + 1] = unJeu[i + 2] = new carte();
         break;
     }
 }

 for (int i = 0; i < unJeu.Length - 1; i++)
 {
     if (unJeu[i].valeur == unJeu[i + 1].valeur)
     {
         hasPaire = true;
         break;
     }
 }

 if (hasBrelan && hasPaire)
 {
     return combinaison.FULL;
 }
 // Vérifier s'il y a une suite (quinte)
 Array.Sort(unJeu, (x, y) => x.valeur.CompareTo(y.valeur));

 bool hasSuite = true;
 for (int i = 0; i < unJeu.Length - 1; i++)
 {
     if (unJeu[i].valeur + 1 != unJeu[i + 1].valeur)
     {
         hasSuite = false;
         break;
     }
 }

 if (hasSuite)
 {
     // Vérifier si toutes les cartes ont la même famille (quinte flush)
     bool hasFlush = true;
     for (int i = 0; i < unJeu.Length - 1; i++)
     {
         if (unJeu[i].famille != unJeu[i + 1].famille)
         {
             hasFlush = false;
             break;
         }
     }

     if (hasFlush)
     {
         return combinaison.QUINTE_FLUSH;
     }
 }

 return combinaison.RIEN; // Retourner RIEN si aucune combinaison n'est trouvée
        }

        private static void EchangeCarte(carte[] unJeu, int[] e)
        {
            Console.WriteLine("Voulez-vous échanger des cartes ? (O/N)");
            char choix = Console.ReadKey().KeyChar;

            if (choix == 'O' || choix == 'o')
            {
                Console.WriteLine("Entrez les numéros des cartes à échanger (1 à 5), séparés par des espaces :");
                string[] numerosStr = Console.ReadLine().Split(' ');
                int numero;
                for (int i = 0; i < numerosStr.Length; i++)
                {
                    if (int.TryParse(numerosStr[i], out numero) && (numero >= 1 && numero <= 5))
                    {
                        // Les numéros sont valides, marquer la carte à échanger
                        echange[numero - 1] = 1;
                        unJeu[i] = Tirage();
                        Console.WriteLine("Carte " + numero + " marquée pour échange.");
                    }
                    else
                    {
                        Console.WriteLine("Entrée invalide pour la carte {numerosStr[i]}. Ignorée.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Aucune carte n'a été échangée.");
            }
        }

        private static void AfficheMenu()
        {
            Console.WriteLine("1. Jouer au poker");
            Console.WriteLine("2. Voir les scores");
            Console.WriteLine("3. Quitter");
        }

        private static void JouerAuPoker()
{
        	
		    TirageDuJeu(MonJeu);
		    AffichageCarte();
		    EchangeCarte(MonJeu, echange);
		    AffichageCarte();
		    AfficheResultat(MonJeu);
		    enregistrerJeu(MonJeu);
		    Console.ReadKey(); // Ajout pour attendre une touche avant de revenir au menu
}

        private static void TirageDuJeu(carte[] unJeu)
        {
            for (int i = 0; i < 5; i++)
            {
                do
                {
                    unJeu[i] = Tirage();
                } while (!CarteUnique(unJeu[i], unJeu, i));
            }
        }

        private static void AffichageCarte()
        {
            // Implémentez l'affichage des cartes ici
            {

    int left = 0;
    int c = 1;

    // Tirage aléatoire de 5 cartes
    for (int i = 0; i < 5; i++)
    {
        // Tirage de la carte n°i (le jeu doit être sans doublons !)

        // Affichage de la carte
        if (MonJeu[i].famille == 3 || MonJeu[i].famille == 4)
            SetConsoleTextAttribute(hConsole, 252);
        else
            SetConsoleTextAttribute(hConsole, 240);
        Console.SetCursorPosition(left, 5);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
        Console.SetCursorPosition(left, 6);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
        Console.SetCursorPosition(left, 7);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
        Console.SetCursorPosition(left, 8);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
        Console.SetCursorPosition(left, 9);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', ' ', '|');
        Console.SetCursorPosition(left, 10);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', (char)MonJeu[i].famille, '|');
        Console.SetCursorPosition(left, 11);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', ' ', '|');
        Console.SetCursorPosition(left, 12);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
        Console.SetCursorPosition(left, 13);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
        Console.SetCursorPosition(left, 14);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
        Console.SetCursorPosition(left, 15);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
        Console.SetCursorPosition(left, 16);
        SetConsoleTextAttribute(hConsole, 10);
        Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", ' ', ' ', ' ', ' ', ' ', c, ' ', ' ', ' ', ' ', ' ');
        left = left + 15;
        c++;
    }

}
        }

        private static void AfficheResultat(carte[] MonJeu)
        {
            Console.WriteLine("Résultat du jeu :");
            ChercheCombinaison(ref MonJeu);
            // Implémentez l'affichage du résultat ici
            SetConsoleTextAttribute(hConsole, 012);
Console.Write("RESULTAT - Vous avez : ");
try
{
    // Test de la combinaison
    switch (ChercheCombinaison(ref MonJeu))
    {
        case combinaison.RIEN:
            Console.WriteLine("rien du tout... desole!"); break;
        case combinaison.PAIRE:
            Console.WriteLine("une simple paire..."); break;
        case combinaison.DOUBLE_PAIRE:
            Console.WriteLine("une double paire; on peut esperer..."); break;
        case combinaison.BRELAN:
            Console.WriteLine("un brelan; pas mal..."); break;
        case combinaison.QUINTE:
            Console.WriteLine("une quinte; bien!"); break;
        case combinaison.FULL:
            Console.WriteLine("un full; ouahh!"); break;
        case combinaison.COULEUR:
            Console.WriteLine("une couleur; bravo!"); break;
        case combinaison.CARRE:
            Console.WriteLine("un carre; champion!"); break;
        case combinaison.QUINTE_FLUSH:
            Console.WriteLine("une quinte-flush; royal!"); break;
    };
    AffichageCarte();
}
catch { }
        }

private static void enregistrerJeu(carte[] unJeu)
        {    
                SetConsoleTextAttribute(hConsole, 010);
                Console.Write("Entrez votre nom ou pseudo : ");
                string nom = Console.ReadLine();

                // Création ou ouverture du fichier scores.txt en mode écriture (FileMode.Append)
                using (StreamWriter writer = new StreamWriter("scores.txt", true))
                {
                    // Format de l'enregistrement : nom;famille1valeur1;famille2valeur2;...;famille5valeur5
                        writer.Write(nom);
                    foreach (carte carte in unJeu)
                    {
                        writer.Write(carte.famille + carte.valeur);
                    }
                    writer.WriteLine(); // Passer à une nouvelle ligne pour le prochain enregistrement
                }

                    Console.WriteLine("Le jeu a été enregistré avec succès !");
            }

  private static void VoirScores()
        {
              try
            {
                // Lecture du fichier scores.txt
                using (StreamReader reader = new StreamReader("scores.txt"))
                {
                    Console.WriteLine("");
                    Console.WriteLine("Scores enregistrés :");

                    // Lire et afficher chaque ligne du fichier
                    while (!reader.EndOfStream)
                    {
                        string ligne = reader.ReadLine();
                        Console.WriteLine(ligne);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Aucun score n'a été enregistré pour le moment.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : {ex.Message}");
            }
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
        	
            while (true)
            {
            	Console.Clear();
                AfficheMenu();
                char reponse = Console.ReadKey().KeyChar;

                switch (reponse)
                {
                    case '1':
                        JouerAuPoker();
                        break;
                    case '2':
                        VoirScores();
                        break;
                    case '3':
                        return; // Quitter
                    default:
                        Console.WriteLine("Option invalide. Veuillez réessayer.");
                        break;
                }
            }
        }
    }
}