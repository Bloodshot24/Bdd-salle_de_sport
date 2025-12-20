using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


namespace SalleSportApp
{
    public class Connexion
    {
        
        
        private MySqlConnection connection;

        // Constructeur: initialiser la connexion
        public Connexion(string id,string mdp)
        {
             string connectionString =
            "Server=127.0.0.1;Database=salle_sport;Uid="+id+";Pwd="+mdp+";";
        connection = new MySqlConnection(connectionString);
        }

        //   Ouvrir la connexion à la BD
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connexion établie avec la base de données");
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur de connexion: {ex.Message}");
                return false;
            }
        }

        //   Fermer la connexion
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                Console.WriteLine("Connexion fermée");
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur lors de la fermeture: {ex.Message}");
                return false;
            }
        }

        //  Obtenir la connexion
        public MySqlConnection GetConnection()
        {
            return connection;
        }
    }

    public class Admin
    {
        private int idAdmin;
        private string nomAdmin;
        private int niveauPrivilege;
        private MySqlConnection connection;

        // initialiser l'administrateur
        public Admin(int id, string nom, int privilege, MySqlConnection conn)
        {
            idAdmin = id;
            nomAdmin = nom;
            niveauPrivilege = privilege;
            connection = conn;
        }

       
        public void AfficherInscriptionsEnAttente()
        {
            Console.WriteLine("\n--- DEMANDES D'INSCRIPTION EN ATTENTE ---");
            
            string requete = "SELECT ID_Membre, Nom, Prenom, Mail, Date_Inscription FROM Membre WHERE Validite_Inscription = FALSEORDER BY Date_Inscription ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucune demande en attente.");
                    reader.Close();
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader.GetInt32(0)} | " +
                        $"Nom: {reader.GetString(1)} {reader.GetString(2)} | " +
                        $"Email: {reader.GetString(3)} | " +
                        $"Date: {reader.GetDateTime(4):dd/MM/yyyy}");
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        public void ValiderInscription(int idMembre)
        {
            // Vérification
            string test = "SELECT Validite_Inscription FROM Membre WHERE ID_Membre = @id";
            MySqlCommand checkCmd = new MySqlCommand(test, connection);
            checkCmd.Parameters.AddWithValue("@id", idMembre);

            try
            {
                object resultat = checkCmd.ExecuteScalar();
                if (resultat == null)
                {
                    Console.WriteLine("Membre non trouvé.");
                    return;
                }

                // Mise à jour: Passer Validite_Inscription à TRUE
                string maj_requete = "UPDATE Membre SET Validite_Inscription = TRUE WHERE ID_Membre = @id";
                MySqlCommand updateCmd = new MySqlCommand(maj_requete, connection);
                updateCmd.Parameters.AddWithValue("@id", idMembre);
                updateCmd.ExecuteNonQuery();

                Console.WriteLine($"Inscription du membre ID {idMembre} validée");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Supprimer une adhésion (membre)
        public void SupprimerMembre(int idMembre)
        {
            try
            {
                // Avant de supprimer le membre, supprimer ses inscriptions aux cours
                string deleteInscriptions = 
                    "DELETE FROM Inscription WHERE ID_Membre = @id";
                MySqlCommand deleteInsCmd = 
                    new MySqlCommand(deleteInscriptions, connection);
                deleteInsCmd.Parameters.AddWithValue("@id", idMembre);
                deleteInsCmd.ExecuteNonQuery();

                // Supprimer le membre lui-même
                string deleteMember = "DELETE FROM Membre WHERE ID_Membre = @id";
                MySqlCommand deleteMemCmd = new MySqlCommand(deleteMember, connection);
                deleteMemCmd.Parameters.AddWithValue("@id", idMembre);
                int rowsAffected = deleteMemCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Console.WriteLine($"  Membre ID {idMembre} supprimé");
                else
                    Console.WriteLine("Membre non trouvé.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Afficher les informations d'un membre
        public void AfficherInfoMembre(int idMembre)
        {
            string requete = "SELECT ID_Membre, Nom, Prenom, Adresse, Tel, Mail, Date_Inscription, Validite_Inscription FROM Membre WHERE ID_Membre = @id";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@id", idMembre);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    Console.WriteLine("Membre non trouvé.");
                    reader.Close();
                    return;
                }

                Console.WriteLine($"\n--- INFORMATIONS MEMBRE ---");
                Console.WriteLine($"ID: {reader.GetInt32(0)}");
                Console.WriteLine($"Nom: {reader.GetString(1)} {reader.GetString(2)}");
                Console.WriteLine($"Adresse: {reader.GetString(3)}");
                Console.WriteLine($"Téléphone: {reader.GetString(4)}");
                Console.WriteLine($"Email: {reader.GetString(5)}");
                Console.WriteLine($"Date inscription: {reader.GetDateTime(6):dd/MM/yyyy}");
                Console.WriteLine($"Statut: {(reader.GetBoolean(7) ? "Valide" : "En attente")}");

                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Modifier les informations d'un membre
        public void ModifierMembre(int idMembre, string nom, string prenom,string adresse, string tel, string mail)
        {
            string requete = "UPDATE Membre SET Nom = @nom, Prenom = @prenom, Adresse = @adresse, Tel = @tel, Mail = @mail WHERE ID_Membre = @id";
            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@tel", tel);
                cmd.Parameters.AddWithValue("@mail", mail);
                cmd.Parameters.AddWithValue("@id", idMembre);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine(" Informations du membre mises à jour");
                else
                    Console.WriteLine("Membre non trouvé.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
        // Ajouter un nouveau coach
        public void AjouterCoach(string nom, string prenom, string tel,string mdp, string formation, string specialite)
        {
            string requete = "INSERT INTO Coach (Nom, Prenom, Tel, MDP, Formation, Specialite) VALUES (@nom, @prenom, @tel, @mdp, @formation, @specialite)";
        
            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@tel", tel);
                cmd.Parameters.AddWithValue("@mdp", mdp);
                cmd.Parameters.AddWithValue("@formation", formation);
                cmd.Parameters.AddWithValue("@specialite", specialite);

                cmd.ExecuteNonQuery();
                Console.WriteLine($" Coach {nom} {prenom} ajouté");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Afficher tous les coachs
        public void AfficherCoachs()
        {
            Console.WriteLine("\n--- LISTE DES COACHS ---");
            string requete = "SELECT ID_Coach, Nom, Prenom, Tel, Formation, Specialite FROM Coach ORDER BY Nom ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucun coach trouvé.");
                    reader.Close();
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader.GetInt32(0)} | " +
                        $"Nom: {reader.GetString(1)} {reader.GetString(2)} | " +
                        $"Formation: {reader.GetString(4)} | " +
                        $"Spécialité: {reader.GetString(5)}");
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }


        //   Afficher tous les cours
        public void AfficherCours()
        {
            Console.WriteLine("\n--- LISTE DES COURS ---");
            string requete =  "SELECT ID_Cours, Nom_Cours, Description, Duree_h_min, Intensite, Difficulte, Capacite FROM Cours ORDER BY Nom_Cours ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucun cours trouvé.");
                    reader.Close();
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader.GetInt32(0)} | " +
                        $"Nom: {reader.GetString(1)} | " +
                        $"Durée: {reader.GetDecimal(3)}h | " +
                        $"Intensité: {reader.GetInt32(4)}/5 | " +
                        $"Difficulté: {reader.GetString(5)} | " +
                        $"Capacité: {reader.GetInt32(6)} personnes");
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        //   Afficher les statistiques (requête avec agrégations)
        public void AfficherStatistiques()
        {
            Console.WriteLine("\n--- STATISTIQUES DE LA SALLE ---");

            // Statistique 1: Nombre total de membres
            string requete_membre = "SELECT COUNT(*) FROM Membre";
            MySqlCommand cmdMembers = new MySqlCommand(requete_membre, connection);
            int totalMembers = Convert.ToInt32(cmdMembers.ExecuteScalar() ?? 0);
            Console.WriteLine($"Total de membres: {totalMembers}");

            // Statistique 2: Nombre de cours disponibles
            string requete_cours = "SELECT COUNT(*) FROM Cours";
            MySqlCommand cmdCourses = new MySqlCommand(requete_cours, connection);
            int totalCourses = Convert.ToInt32(cmdCourses.ExecuteScalar() ?? 0);
            Console.WriteLine($"Total de cours: {totalCourses}");

            // Statistique 3: Nombre de coachs
            string requete_coaches = "SELECT COUNT(*) FROM Coach";
            MySqlCommand cmdCoaches = new MySqlCommand(requete_coaches, connection);
            int totalCoaches = Convert.ToInt32(cmdCoaches.ExecuteScalar() ?? 0);
            Console.WriteLine($"Total de coachs: {totalCoaches}");

            // Statistique 4: Cours avec le plus d'inscriptions
            string requete_nombre_inscriptions =  "SELECT c.Nom_Cours, COUNT(i.ID_Membre) as NbInscrits FROM Cours c LEFT JOIN Inscription i ON c.ID_Cours = i.ID_Cours GROUP BY c.ID_Cours, c.Nom_Cours ORDER BY NbInscrits DESC LIMIT 1";
            
            MySqlCommand cmdPopular = new MySqlCommand(requete_nombre_inscriptions, connection);
            MySqlDataReader readerPopular = cmdPopular.ExecuteReader();
            if (readerPopular.Read())
            {
                Console.WriteLine($"Cours le plus populaire: " +
                    $"{readerPopular.GetString(0)} " +
                    $"({readerPopular.GetInt32(1)} inscrits)");
            }
            readerPopular.Close();
        }
    }

    public class Membre
    {
        private int idMembre;
        private string nomMembre;
        private bool validite;
        private MySqlConnection connection;

        public Membre(int id, string nom, bool valide, MySqlConnection conn)
        {
            idMembre = id;
            nomMembre = nom;
            validite = valide;
            connection = conn;
        }

        // S'inscrire à un cours (réservation)
        public void InscrireACours(int idCours)
        {
            try
            {
                // Vérification
                if (!validite)
                {
                    Console.WriteLine("Votre inscription n'a pas encore été validée");
                    return;
                }

                // Vérification
                string checkCourse = "SELECT Capacite FROM Cours WHERE ID_Cours = @idCours";
                MySqlCommand checkCmd = new MySqlCommand(checkCourse, connection);
                checkCmd.Parameters.AddWithValue("@idCours", idCours);
                object capacite = checkCmd.ExecuteScalar();

                if (capacite == null)
                {
                    Console.WriteLine("Cours non trouvé");
                    return;
                }

                // Vérification 3: Y a-t-il de la place dans le cours?
                string countInscriptions =  "SELECT COUNT(*) FROM Inscription WHERE ID_Cours = @idCours";
                MySqlCommand countCmd = new MySqlCommand(countInscriptions, connection);
                countCmd.Parameters.AddWithValue("@idCours", idCours);
                int nbInscrits = Convert.ToInt32(countCmd.ExecuteScalar() ?? 0);
                if (nbInscrits >= Convert.ToInt32(capacite))
                {
                    Console.WriteLine("Le cours est complet!");
                    return;
                }

                // Vérification 4: N'est-il pas déjà inscrit?
                string checkExist =  "SELECT ID_Membre FROM Inscription WHERE ID_Membre = @idMembre AND ID_Cours = @idCours";
                MySqlCommand existCmd = new MySqlCommand(checkExist, connection);
                existCmd.Parameters.AddWithValue("@idMembre", idMembre);
                existCmd.Parameters.AddWithValue("@idCours", idCours);
                object exist = existCmd.ExecuteScalar();

                if (exist != null)
                {
                    Console.WriteLine("Vous êtes déjà inscrit à ce cours");
                    return;
                }

                // Insertion: Ajouter l'inscription
                string requete =  "INSERT INTO Inscription (ID_Membre, ID_Cours, Date_Inscription) VALUES (@idMembre, @idCours, NOW())";
                MySqlCommand insertCmd = new MySqlCommand(requete, connection);
                insertCmd.Parameters.AddWithValue("@idMembre", idMembre);
                insertCmd.Parameters.AddWithValue("@idCours", idCours);
                insertCmd.ExecuteNonQuery();

                Console.WriteLine("Inscription au cours réussie!");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Voir les cours disponibles
        public void AfficherCoursDisponibles()
        {
            Console.WriteLine("\n--- COURS DISPONIBLES ---");
            string requete =  "SELECT c.ID_Cours, c.Nom_Cours, c.Description, c.Duree_h_min, c.Intensite, c.Difficulte, c.Capacite, COUNT(i.ID_Membre) as NbInscrits FROM Cours c LEFT JOIN Inscription i ON c.ID_Cours = i.ID_Cours GROUP BY c.ID_Cours ORDER BY c.Nom_Cours ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Aucun cours disponible.");
                    reader.Close();
                    return;
                }

                while (reader.Read())
                {
                    int nbInscrits = reader.GetInt32(7);
                    int capacite = reader.GetInt32(6);
                    int placesRestantes = capacite - nbInscrits;

                    Console.WriteLine($"\nID: {reader.GetInt32(0)}");
                    Console.WriteLine($"Nom: {reader.GetString(1)}");
                    Console.WriteLine($"Description: {reader.GetString(2)}");
                    Console.WriteLine($"Durée: {reader.GetDecimal(3)}h");
                    Console.WriteLine($"Intensité: {reader.GetInt32(4)}/5");
                    Console.WriteLine($"Difficulté: {reader.GetString(5)}");
                    Console.WriteLine($"Places: {placesRestantes}/{capacite}");
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        //   Voir mes inscriptions
        public void AfficherMesInscriptions()
        {
            Console.WriteLine("\n--- MES INSCRIPTIONS ---");
            string requete =  "SELECT c.Nom_Cours, c.Description, c.Duree_h_min, c.Difficulte, i.Date_Inscription FROM Inscription i JOIN Cours c ON i.ID_Cours = c.ID_Cours WHERE i.ID_Membre = @idMembre ORDER BY i.Date_Inscription DESC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@idMembre", idMembre);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Vous n'êtes inscrit à aucun cours.");
                    reader.Close();
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"Cours: {reader.GetString(0)}");
                    Console.WriteLine($"Description: {reader.GetString(1)}");
                    Console.WriteLine($"Durée: {reader.GetDecimal(2)}h");
                    Console.WriteLine($"Difficulté: {reader.GetString(3)}");
                    Console.WriteLine($"Date d'inscription: {reader.GetDateTime(4):dd/MM/yyyy}");
                    Console.WriteLine("---");
                }
                reader.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Annuler une inscription
        public void AnnulerInscription(int idCours)
        {
            string requete =  "DELETE FROM Inscription WHERE ID_Membre = @idMembre AND ID_Cours = @idCours";

            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@idMembre", idMembre);
                cmd.Parameters.AddWithValue("@idCours", idCours);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine(" Inscription annulée");
                else
                    Console.WriteLine("Inscription non trouvée.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
    }

  
    public class Authentification
    {
        private MySqlConnection connection;

        public Authentification(MySqlConnection conn)
        {
            connection = conn;
        }

        // Authentifier un administrateur
        public Admin AuthifierAdmin(string identifiant, string mdp)
        {
            string requete =  "SELECT ID_Admin, CONCAT(Nom, ' ', Prenom), Nv_Privilege FROM Admin WHERE CONCAT(Nom, ' ', Prenom) = @id AND MDP = @mdp";   
        
            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@id", identifiant);
                cmd.Parameters.AddWithValue("@mdp", mdp);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int idAdmin = reader.GetInt32(0);
                    string nomAdmin = reader.GetString(1);
                    int privilege = reader.GetInt32(2);
                    reader.Close();
                    return new Admin(idAdmin, nomAdmin, privilege, connection);
                }
                reader.Close();
                return null;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                return null;
            }
        }

        //   Authentifier un membre
        public Membre AuthentifierMembre(string mail, string mdp)
        {
            string requete =  "SELECT ID_Membre, CONCAT(Nom, ' ', Prenom), Validite_Inscription FROM Membre WHERE Mail = @mail AND MDP = @mdp";
    
            try
            {
                MySqlCommand cmd = new MySqlCommand(requete, connection);
                cmd.Parameters.AddWithValue("@mail", mail);
                cmd.Parameters.AddWithValue("@mdp", mdp);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int idMembre = reader.GetInt32(0);
                    string nomMembre = reader.GetString(1);
                    bool validite = reader.GetBoolean(2);
                    reader.Close();
                    return new Membre(idMembre, nomMembre, validite, connection);
                }
                reader.Close();
                return null;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                return null;
            }
        }

        //   Inscrire un nouveau membre
        public bool InscrireNouveauMembre(string nom, string prenom, string adresse, string tel, string mail, string mdp)
        {
            string test = "SELECT Mail FROM Membre WHERE Mail = @mail";
            MySqlCommand checkCmd = new MySqlCommand(test, connection);
            checkCmd.Parameters.AddWithValue("@mail", mail);

            try
            {
                object resultat = checkCmd.ExecuteScalar();
                if (resultat != null)
                {
                    Console.WriteLine("Cet email est déjà utilisé");
                    return false;
                }

                // Insertion: Ajouter le nouveau membre
                string requete =  "INSERT INTO Membre (Nom, Prenom, Adresse, Tel, Mail, Date_Inscription, Validite_Inscription, MDP, ID_Admin)VALUES (@nom, @prenom, @adresse, @tel, @mail, NOW(),FALSE, @mdp, NULL)";

                MySqlCommand insertCmd = new MySqlCommand(requete, connection);
                insertCmd.Parameters.AddWithValue("@nom", nom);
                insertCmd.Parameters.AddWithValue("@prenom", prenom);
                insertCmd.Parameters.AddWithValue("@adresse", adresse);
                insertCmd.Parameters.AddWithValue("@tel", tel);
                insertCmd.Parameters.AddWithValue("@mail", mail);
                insertCmd.Parameters.AddWithValue("@mdp", mdp);

                insertCmd.ExecuteNonQuery();
                Console.WriteLine(" Inscription enregistrée. " +
                    "En attente de validation par un administrateur.");
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                return false;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Créer la connexion à la base de données
            Console.Clear();
                Console.WriteLine("╔══════════════ MENU CONNEXION ═════════════╗");
                Console.WriteLine("║ Entrez votre identifiant                  ║");
                Console.WriteLine("║ Entrez votre mot de passe                 ║");
                Console.WriteLine("╚═══════════════════════════════════════════╝");
            string id = Console.ReadLine() ?? "root";
            string mdp = "";
            while (true)
            {
                var touche = Console.ReadKey(true);
                if (touche.Key == ConsoleKey.Enter)
                    break;
                mdp += touche.KeyChar;
                Console.Write("*");
            }
            Console.WriteLine();


            Connexion dbConnection = new Connexion(id, mdp);

            if (!dbConnection.OpenConnection())
            {
                Console.WriteLine("Impossible de se connecter à la base de données");
                return;
            }

            // Navigation de l'application
            bool isRunning = true;
            MySqlConnection connection = dbConnection.GetConnection();

            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("====================================");
                Console.WriteLine("  BIENVENUE - GESTION SALLE SPORT");
                Console.WriteLine("====================================");
                Console.WriteLine("Veuillez saisir votre choix:");
                Console.WriteLine("1- Je suis membre je me connecte");
                Console.WriteLine("2- Je m'inscris");
                Console.WriteLine("3- Je suis administrateur");
                Console.WriteLine("0- Quitter");
                Console.WriteLine("====================================");

                string choice = Console.ReadLine();
                Authentification auth = new Authentification(connection);

                switch (choice)
                {
                    case "1":
                        // Connexion membre
                        Console.WriteLine("\n--- CONNEXION MEMBRE ---");
                        Console.Write("Email: ");
                        string mail = Console.ReadLine();
                        Console.Write("Mot de passe: ");
                        string mdpMembre = Console.ReadLine();

                        Membre membre = auth.AuthentifierMembre(mail, mdpMembre);
                        if (membre != null)
                        {
                            MenuMembre(membre);
                        }
                        else
                        {
                            Console.WriteLine("Email ou mot de passe incorrect");
                            Console.ReadKey();
                        }
                        break;

                    case "2":
                        // Inscription nouveau membre
                        Console.WriteLine("\n--- INSCRIPTION ---");
                        Console.Write("Nom: ");
                        string nomIns = Console.ReadLine();
                        Console.Write("Prénom: ");
                        string prenomIns = Console.ReadLine();
                        Console.Write("Adresse: ");
                        string adresseIns = Console.ReadLine();
                        Console.Write("Téléphone: ");
                        string telIns = Console.ReadLine();
                        Console.Write("Email: ");
                        string mailIns = Console.ReadLine();
                        Console.Write("Mot de passe: ");
                        string mdpIns = Console.ReadLine();

                        auth.InscrireNouveauMembre(nomIns, prenomIns, adresseIns, 
                                                   telIns, mailIns, mdpIns);
                        Console.ReadKey();
                        break;

                    case "3":
                        // Connexion administrateur
                        Console.WriteLine("\n--- CONNEXION ADMINISTRATEUR ---");
                        Console.Write("Identifiant (Nom Prénom): ");
                        string identifiant = Console.ReadLine();
                        Console.Write("Mot de passe: ");
                        string mdpAdmin = Console.ReadLine();

                        Admin admin = auth.AuthifierAdmin(identifiant, mdpAdmin);
                        if (admin != null)
                        {
                            MenuAdmin(admin);
                        }
                        else
                        {
                            Console.WriteLine("Identifiants incorrects");
                            Console.ReadKey();
                        }
                        break;

                    case "0":
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Choix invalide");
                        Console.ReadKey();
                        break;
                }
            }

            // Fermer la connexion
            dbConnection.CloseConnection();
            Console.WriteLine("Au revoir!");
        }

        //   Menu pour les administrateurs
        static void MenuAdmin(Admin admin)
        {
            bool inAdminMenu = true;
            while (inAdminMenu)
            {
                Console.Clear();
                Console.WriteLine("====================================");
                Console.WriteLine("  INTERFACE ADMINISTRATEUR");
                Console.WriteLine("====================================");
                Console.WriteLine("1- Afficher les inscriptions en attente");
                Console.WriteLine("2- Valider une inscription");
                Console.WriteLine("3- Supprimer un membre");
                Console.WriteLine("4- Afficher info d'un membre");
                Console.WriteLine("5- Modifier un membre");
                Console.WriteLine("6- Ajouter un coach");
                Console.WriteLine("7- Afficher tous les coachs");
                Console.WriteLine("8- Afficher tous les cours");
                Console.WriteLine("9- Afficher statistiques");
                Console.WriteLine("0- Déconnexion");
                Console.WriteLine("====================================");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        admin.AfficherInscriptionsEnAttente();
                        Console.ReadKey();
                        break;
                    case "2":
                        Console.Write("ID du membre: ");
                        if (int.TryParse(Console.ReadLine(), out int idMembre))
                            admin.ValiderInscription(idMembre);
                        Console.ReadKey();
                        break;
                    case "3":
                        Console.Write("ID du membre à supprimer: ");
                        if (int.TryParse(Console.ReadLine(), out int idMembreSup))
                            admin.SupprimerMembre(idMembreSup);
                        Console.ReadKey();
                        break;
                    case "4":
                        Console.Write("ID du membre: ");
                        if (int.TryParse(Console.ReadLine(), out int idMembreInfo))
                            admin.AfficherInfoMembre(idMembreInfo);
                        Console.ReadKey();
                        break;
                    case "5":
                        Console.Write("ID du membre: ");
                        if (int.TryParse(Console.ReadLine(), out int idMembreMod))
                        {
                            Console.Write("Nouveau nom: ");
                            string nomMod = Console.ReadLine();
                            Console.Write("Nouveau prénom: ");
                            string prenomMod = Console.ReadLine();
                            Console.Write("Nouvelle adresse: ");
                            string adresseMod = Console.ReadLine();
                            Console.Write("Nouveau téléphone: ");
                            string telMod = Console.ReadLine();
                            Console.Write("Nouvel email: ");
                            string mailMod = Console.ReadLine();
                            admin.ModifierMembre(idMembreMod, nomMod, prenomMod, 
                                                adresseMod, telMod, mailMod);
                        }
                        Console.ReadKey();
                        break;
                    case "6":
                        Console.Write("Nom: ");
                        string nomCoach = Console.ReadLine();
                        Console.Write("Prénom: ");
                        string prenomCoach = Console.ReadLine();
                        Console.Write("Téléphone: ");
                        string telCoach = Console.ReadLine();
                        Console.Write("Mot de passe: ");
                        string mdpCoach = Console.ReadLine();
                        Console.Write("Formation: ");
                        string formationCoach = Console.ReadLine();
                        Console.Write("Spécialité: ");
                        string specialiteCoach = Console.ReadLine();
                        admin.AjouterCoach(nomCoach, prenomCoach, telCoach, 
                                          mdpCoach, formationCoach, specialiteCoach);
                        Console.ReadKey();
                        break;
                    case "7":
                        admin.AfficherCoachs();
                        Console.ReadKey();
                        break;
                    case "8":
                        admin.AfficherCours();
                        Console.ReadKey();
                        break;
                    case "9":
                        admin.AfficherStatistiques();
                        Console.ReadKey();
                        break;
                    case "0":
                        inAdminMenu = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide");
                        Console.ReadKey();
                        break;
                }
            }
        }

        //   Menu pour les membres
        static void MenuMembre(Membre membre)
        {
            bool inMembreMenu = true;
            while (inMembreMenu)
            {
                Console.Clear();
                Console.WriteLine("====================================");
                Console.WriteLine("  INTERFACE MEMBRE");
                Console.WriteLine("====================================");
                Console.WriteLine("1- Voir les cours disponibles");
                Console.WriteLine("2- S'inscrire à un cours");
                Console.WriteLine("3- Voir mes inscriptions");
                Console.WriteLine("4- Annuler une inscription");
                Console.WriteLine("0- Déconnexion");
                Console.WriteLine("====================================");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        membre.AfficherCoursDisponibles();
                        Console.ReadKey();
                        break;
                    case "2":
                        Console.Write("ID du cours: ");
                        if (int.TryParse(Console.ReadLine(), out int idCours))
                            membre.InscrireACours(idCours);
                        Console.ReadKey();
                        break;
                    case "3":
                        membre.AfficherMesInscriptions();
                        Console.ReadKey();
                        break;
                    case "4":
                        Console.Write("ID du cours à annuler: ");
                        if (int.TryParse(Console.ReadLine(), out int idCoursAnnuler))
                            membre.AnnulerInscription(idCoursAnnuler);
                        Console.ReadKey();
                        break;
                    case "0":
                        inMembreMenu = false;
                        break;
                    default:
                        Console.WriteLine("Choix invalide");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}