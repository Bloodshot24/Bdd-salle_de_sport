using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

// ============================================================================
// PROJET: GESTION SALLE DE SPORT - C#
// ============================================================================
// Ce programme gère l'interface de connexion et navigation pour une salle 
// de sport avec trois rôles: Administrateur, Coach et Membre
// ============================================================================


namespace SalleSportApp
{
    // ========================================================================
    // CLASSE: DatabaseConnection
    // RÔLE: Gérer la connexion à la base de données MySQL
    // ========================================================================
    public class DatabaseConnection
    {
        // Chaîne de connexion: paramètres de connexion au serveur MySQL
        private string connectionString = 
            "Server=localhost;Database=salle_sport;Uid=root;Pwd=root;";
        
        private MySqlConnection connection;

        // Constructeur: initialiser la connexion
        public DatabaseConnection()
        {
            connection = new MySqlConnection(connectionString);
        }

        // Méthode: Ouvrir la connexion à la BD
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                Console.WriteLine("[✓] Connexion établie avec la base de données");
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[✗] Erreur de connexion: {ex.Message}");
                return false;
            }
        }

        // Méthode: Fermer la connexion
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                Console.WriteLine("[✓] Connexion fermée");
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[✗] Erreur lors de la fermeture: {ex.Message}");
                return false;
            }
        }

        // Propriété: Retourner l'objet MySqlConnection pour utilisation
        public MySqlConnection GetConnection()
        {
            return connection;
        }
    }

    // ========================================================================
    // CLASSE: Admin
    // RÔLE: Gérer les opérations de l'administrateur
    // ========================================================================
    public class Admin
    {
        private int idAdmin;
        private string nomAdmin;
        private int niveauPrivilege;
        private MySqlConnection connection;

        // Constructeur: initialiser l'administrateur
        public Admin(int id, string nom, int privilege, MySqlConnection conn)
        {
            idAdmin = id;
            nomAdmin = nom;
            niveauPrivilege = privilege;
            connection = conn;
        }

        // ====================================================================
        // GESTION DES MEMBRES
        // ====================================================================

        // Méthode: Afficher les demandes d'inscription en attente
        public void AfficherInscriptionsEnAttente()
        {
            Console.WriteLine("\n--- DEMANDES D'INSCRIPTION EN ATTENTE ---");
            
            string query = @"
                SELECT ID_Membre, Nom, Prenom, Mail, Date_Inscription 
                FROM Membre 
                WHERE Validite_Inscription = FALSE
                ORDER BY Date_Inscription ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Valider une inscription de membre
        public void ValiderInscription(int idMembre)
        {
            // Vérification: Le membre existe-t-il?
            string checkQuery = "SELECT Validite_Inscription FROM Membre WHERE ID_Membre = @id";
            MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@id", idMembre);

            try
            {
                object result = checkCmd.ExecuteScalar();
                if (result == null)
                {
                    Console.WriteLine("Membre non trouvé.");
                    return;
                }

                // Mise à jour: Passer Validite_Inscription à TRUE
                string updateQuery = 
                    "UPDATE Membre SET Validite_Inscription = TRUE WHERE ID_Membre = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@id", idMembre);
                updateCmd.ExecuteNonQuery();

                Console.WriteLine($"[✓] Inscription du membre ID {idMembre} validée");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Supprimer une adhésion (membre)
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
                    Console.WriteLine($"[✓] Membre ID {idMembre} supprimé");
                else
                    Console.WriteLine("Membre non trouvé.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Afficher les informations d'un membre
        public void AfficherInfoMembre(int idMembre)
        {
            string query = @"
                SELECT ID_Membre, Nom, Prenom, Adresse, Tel, Mail, 
                       Date_Inscription, Validite_Inscription 
                FROM Membre 
                WHERE ID_Membre = @id";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Modifier les informations d'un membre
        public void ModifierMembre(int idMembre, string nom, string prenom, 
                                   string adresse, string tel, string mail)
        {
            string query = @"
                UPDATE Membre 
                SET Nom = @nom, Prenom = @prenom, Adresse = @adresse, 
                    Tel = @tel, Mail = @mail 
                WHERE ID_Membre = @id";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@tel", tel);
                cmd.Parameters.AddWithValue("@mail", mail);
                cmd.Parameters.AddWithValue("@id", idMembre);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine("[✓] Informations du membre mises à jour");
                else
                    Console.WriteLine("Membre non trouvé.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // ====================================================================
        // GESTION DES COACHS
        // ====================================================================

        // Méthode: Ajouter un nouveau coach
        public void AjouterCoach(string nom, string prenom, string tel, 
                                 string mdp, string formation, string specialite)
        {
            string query = @"
                INSERT INTO Coach (Nom, Prenom, Tel, MDP, Formation, Specialite) 
                VALUES (@nom, @prenom, @tel, @mdp, @formation, @specialite)";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@tel", tel);
                cmd.Parameters.AddWithValue("@mdp", mdp);
                cmd.Parameters.AddWithValue("@formation", formation);
                cmd.Parameters.AddWithValue("@specialite", specialite);

                cmd.ExecuteNonQuery();
                Console.WriteLine($"[✓] Coach {nom} {prenom} ajouté");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Afficher tous les coachs
        public void AfficherCoachs()
        {
            Console.WriteLine("\n--- LISTE DES COACHS ---");
            string query = @"
                SELECT ID_Coach, Nom, Prenom, Tel, Formation, Specialite 
                FROM Coach 
                ORDER BY Nom ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // ====================================================================
        // GESTION DES COURS
        // ====================================================================

        // Méthode: Ajouter un nouveau cours
        public void AjouterCours(string nom, string description, decimal duree, 
                                 int intensite, string difficulte, int capacite)
        {
            string query = @"
                INSERT INTO Cours (Nom_Cours, Description, Duree_h_min, 
                                   Intensite, Difficulte, Capacite) 
                VALUES (@nom, @desc, @duree, @intensite, @difficulte, @capacite)";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.Parameters.AddWithValue("@duree", duree);
                cmd.Parameters.AddWithValue("@intensite", intensite);
                cmd.Parameters.AddWithValue("@difficulte", difficulte);
                cmd.Parameters.AddWithValue("@capacite", capacite);

                cmd.ExecuteNonQuery();
                Console.WriteLine($"[✓] Cours '{nom}' ajouté");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Afficher tous les cours
        public void AfficherCours()
        {
            Console.WriteLine("\n--- LISTE DES COURS ---");
            string query = @"
                SELECT ID_Cours, Nom_Cours, Description, Duree_h_min, 
                       Intensite, Difficulte, Capacite 
                FROM Cours 
                ORDER BY Nom_Cours ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Modifier un cours
        public void ModifierCours(int idCours, string nom, string description, 
                                  decimal duree, int intensite, string difficulte)
        {
            string query = @"
                UPDATE Cours 
                SET Nom_Cours = @nom, Description = @desc, Duree_h_min = @duree, 
                    Intensite = @intensite, Difficulte = @difficulte 
                WHERE ID_Cours = @id";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.Parameters.AddWithValue("@duree", duree);
                cmd.Parameters.AddWithValue("@intensite", intensite);
                cmd.Parameters.AddWithValue("@difficulte", difficulte);
                cmd.Parameters.AddWithValue("@id", idCours);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine("[✓] Cours modifié");
                else
                    Console.WriteLine("Cours non trouvé.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Supprimer un cours
        public void SupprimerCours(int idCours)
        {
            try
            {
                // Avant de supprimer le cours, supprimer les inscriptions
                string deleteInscriptions = 
                    "DELETE FROM Inscription WHERE ID_Cours = @id";
                MySqlCommand deleteInsCmd = 
                    new MySqlCommand(deleteInscriptions, connection);
                deleteInsCmd.Parameters.AddWithValue("@id", idCours);
                deleteInsCmd.ExecuteNonQuery();

                // Supprimer du planning
                string deletePlanning = "DELETE FROM Planning WHERE ID_Cours = @id";
                MySqlCommand deletePlnCmd = 
                    new MySqlCommand(deletePlanning, connection);
                deletePlnCmd.Parameters.AddWithValue("@id", idCours);
                deletePlnCmd.ExecuteNonQuery();

                // Supprimer le cours
                string deleteCourse = "DELETE FROM Cours WHERE ID_Cours = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteCourse, connection);
                deleteCmd.Parameters.AddWithValue("@id", idCours);
                int rowsAffected = deleteCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Console.WriteLine($"[✓] Cours ID {idCours} supprimé");
                else
                    Console.WriteLine("Cours non trouvé.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Afficher les statistiques (requête avec agrégations)
        public void AfficherStatistiques()
        {
            Console.WriteLine("\n--- STATISTIQUES DE LA SALLE ---");

            // Statistique 1: Nombre total de membres
            string queryMembers = "SELECT COUNT(*) FROM Membre";
            MySqlCommand cmdMembers = new MySqlCommand(queryMembers, connection);
            int totalMembers = (int)cmdMembers.ExecuteScalar();
            Console.WriteLine($"Total de membres: {totalMembers}");

            // Statistique 2: Nombre de cours disponibles
            string queryCourses = "SELECT COUNT(*) FROM Cours";
            MySqlCommand cmdCourses = new MySqlCommand(queryCourses, connection);
            int totalCourses = (int)cmdCourses.ExecuteScalar();
            Console.WriteLine($"Total de cours: {totalCourses}");

            // Statistique 3: Nombre de coachs
            string queryCoaches = "SELECT COUNT(*) FROM Coach";
            MySqlCommand cmdCoaches = new MySqlCommand(queryCoaches, connection);
            int totalCoaches = (int)cmdCoaches.ExecuteScalar();
            Console.WriteLine($"Total de coachs: {totalCoaches}");

            // Statistique 4: Cours avec le plus d'inscriptions
            string queryPopular = @"
                SELECT c.Nom_Cours, COUNT(i.ID_Membre) as NbInscrits
                FROM Cours c
                LEFT JOIN Inscription i ON c.ID_Cours = i.ID_Cours
                GROUP BY c.ID_Cours, c.Nom_Cours
                ORDER BY NbInscrits DESC
                LIMIT 1";
            
            MySqlCommand cmdPopular = new MySqlCommand(queryPopular, connection);
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

    // ========================================================================
    // CLASSE: Membre
    // RÔLE: Gérer les opérations des membres
    // ========================================================================
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

        // Méthode: S'inscrire à un cours (réservation)
        public void InscrireACours(int idCours)
        {
            try
            {
                // Vérification 1: Le membre est-il validé?
                if (!validite)
                {
                    Console.WriteLine("[✗] Votre inscription n'a pas encore été validée");
                    return;
                }

                // Vérification 2: Le cours existe-t-il?
                string checkCourse = "SELECT Capacite FROM Cours WHERE ID_Cours = @idCours";
                MySqlCommand checkCmd = new MySqlCommand(checkCourse, connection);
                checkCmd.Parameters.AddWithValue("@idCours", idCours);
                object capacite = checkCmd.ExecuteScalar();

                if (capacite == null)
                {
                    Console.WriteLine("[✗] Cours non trouvé");
                    return;
                }

                // Vérification 3: Y a-t-il de la place dans le cours?
                string countInscriptions = @"
                    SELECT COUNT(*) FROM Inscription 
                    WHERE ID_Cours = @idCours";
                MySqlCommand countCmd = 
                    new MySqlCommand(countInscriptions, connection);
                countCmd.Parameters.AddWithValue("@idCours", idCours);
                int nbInscrits = (int)countCmd.ExecuteScalar();

                if (nbInscrits >= (int)capacite)
                {
                    Console.WriteLine("[✗] Le cours est complet!");
                    return;
                }

                // Vérification 4: N'est-il pas déjà inscrit?
                string checkExist = @"
                    SELECT ID_Membre FROM Inscription 
                    WHERE ID_Membre = @idMembre AND ID_Cours = @idCours";
                MySqlCommand existCmd = new MySqlCommand(checkExist, connection);
                existCmd.Parameters.AddWithValue("@idMembre", idMembre);
                existCmd.Parameters.AddWithValue("@idCours", idCours);
                object exist = existCmd.ExecuteScalar();

                if (exist != null)
                {
                    Console.WriteLine("[✗] Vous êtes déjà inscrit à ce cours");
                    return;
                }

                // Insertion: Ajouter l'inscription
                string insertQuery = @"
                    INSERT INTO Inscription (ID_Membre, ID_Cours, Date_Inscription) 
                    VALUES (@idMembre, @idCours, NOW())";
                MySqlCommand insertCmd = 
                    new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@idMembre", idMembre);
                insertCmd.Parameters.AddWithValue("@idCours", idCours);
                insertCmd.ExecuteNonQuery();

                Console.WriteLine("[✓] Inscription au cours réussie!");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        // Méthode: Voir les cours disponibles
        public void AfficherCoursDisponibles()
        {
            Console.WriteLine("\n--- COURS DISPONIBLES ---");
            string query = @"
                SELECT c.ID_Cours, c.Nom_Cours, c.Description, c.Duree_h_min, 
                       c.Intensite, c.Difficulte, c.Capacite,
                       COUNT(i.ID_Membre) as NbInscrits
                FROM Cours c
                LEFT JOIN Inscription i ON c.ID_Cours = i.ID_Cours
                GROUP BY c.ID_Cours
                ORDER BY c.Nom_Cours ASC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Voir mes inscriptions
        public void AfficherMesInscriptions()
        {
            Console.WriteLine("\n--- MES INSCRIPTIONS ---");
            string query = @"
                SELECT c.Nom_Cours, c.Description, c.Duree_h_min, 
                       c.Difficulte, i.Date_Inscription
                FROM Inscription i
                JOIN Cours c ON i.ID_Cours = c.ID_Cours
                WHERE i.ID_Membre = @idMembre
                ORDER BY i.Date_Inscription DESC";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Annuler une inscription
        public void AnnulerInscription(int idCours)
        {
            string query = @"
                DELETE FROM Inscription 
                WHERE ID_Membre = @idMembre AND ID_Cours = @idCours";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idMembre", idMembre);
                cmd.Parameters.AddWithValue("@idCours", idCours);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    Console.WriteLine("[✓] Inscription annulée");
                else
                    Console.WriteLine("Inscription non trouvée.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
    }

    // ========================================================================
    // CLASSE: Authentification
    // RÔLE: Gérer l'authentification des utilisateurs
    // ========================================================================
    public class Authentification
    {
        private MySqlConnection connection;

        public Authentification(MySqlConnection conn)
        {
            connection = conn;
        }

        // Méthode: Authentifier un administrateur
        public Admin AuthifierAdmin(string identifiant, string mdp)
        {
            string query = @"
                SELECT ID_Admin, CONCAT(Nom, ' ', Prenom), Nv_Privilege 
                FROM Admin 
                WHERE CONCAT(Nom, ' ', Prenom) = @id AND MDP = @mdp";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Authentifier un membre
        public Membre AuthentifierMembre(string mail, string mdp)
        {
            string query = @"
                SELECT ID_Membre, CONCAT(Nom, ' ', Prenom), Validite_Inscription 
                FROM Membre 
                WHERE Mail = @mail AND MDP = @mdp";

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
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

        // Méthode: Inscrire un nouveau membre
        public bool InscrireNouveauMembre(string nom, string prenom, string adresse, 
                                          string tel, string mail, string mdp)
        {
            // Vérification: L'email est-il déjà utilisé?
            string checkQuery = "SELECT Mail FROM Membre WHERE Mail = @mail";
            MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@mail", mail);

            try
            {
                object result = checkCmd.ExecuteScalar();
                if (result != null)
                {
                    Console.WriteLine("[✗] Cet email est déjà utilisé");
                    return false;
                }

                // Insertion: Ajouter le nouveau membre
                string insertQuery = @"
                    INSERT INTO Membre 
                    (Nom, Prenom, Adresse, Tel, Mail, Date_Inscription, 
                     Validite_Inscription, MDP, ID_Admin) 
                    VALUES (@nom, @prenom, @adresse, @tel, @mail, NOW(), 
                            FALSE, @mdp, NULL)";

                MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@nom", nom);
                insertCmd.Parameters.AddWithValue("@prenom", prenom);
                insertCmd.Parameters.AddWithValue("@adresse", adresse);
                insertCmd.Parameters.AddWithValue("@tel", tel);
                insertCmd.Parameters.AddWithValue("@mail", mail);
                insertCmd.Parameters.AddWithValue("@mdp", mdp);

                insertCmd.ExecuteNonQuery();
                Console.WriteLine("[✓] Inscription enregistrée. " +
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

    // ========================================================================
    // CLASSE: Program - Point d'entrée de l'application
    // ========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            // Initialisation: Créer la connexion à la base de données
            DatabaseConnection dbConnection = new DatabaseConnection();

            if (!dbConnection.OpenConnection())
            {
                Console.WriteLine("Impossible de se connecter à la base de données");
                return;
            }

            // Menu principal: Navigation de l'application
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
                            Console.WriteLine("[✗] Email ou mot de passe incorrect");
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
                            Console.WriteLine("[✗] Identifiants incorrects");
                            Console.ReadKey();
                        }
                        break;

                    case "0":
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("[✗] Choix invalide");
                        Console.ReadKey();
                        break;
                }
            }

            // Fermeture: Fermer la connexion
            dbConnection.CloseConnection();
            Console.WriteLine("Au revoir!");
        }

        // Méthode: Menu pour les administrateurs
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
                Console.WriteLine("8- Ajouter un cours");
                Console.WriteLine("9- Afficher tous les cours");
                Console.WriteLine("10- Modifier un cours");
                Console.WriteLine("11- Supprimer un cours");
                Console.WriteLine("12- Afficher statistiques");
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
                        Console.Write("Nom du cours: ");
                        string nomCours = Console.ReadLine();
                        Console.Write("Description: ");
                        string descCours = Console.ReadLine();
                        Console.Write("Durée (en heures): ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal dureeCours))
                        {
                            Console.Write("Intensité (1-5): ");
                            if (int.TryParse(Console.ReadLine(), out int intensiteCours))
                            {
                                Console.Write("Difficulté (Facile/Moyen/Difficile): ");
                                string difficulteCours = Console.ReadLine();
                                Console.Write("Capacité: ");
                                if (int.TryParse(Console.ReadLine(), out int capaciteCours))
                                    admin.AjouterCours(nomCours, descCours, dureeCours, 
                                                     intensiteCours, difficulteCours, capaciteCours);
                            }
                        }
                        Console.ReadKey();
                        break;
                    case "9":
                        admin.AfficherCours();
                        Console.ReadKey();
                        break;
                    case "10":
                        Console.Write("ID du cours à modifier: ");
                        if (int.TryParse(Console.ReadLine(), out int idCoursMod))
                        {
                            Console.Write("Nouveau nom: ");
                            string nomCoursMod = Console.ReadLine();
                            Console.Write("Nouvelle description: ");
                            string descCoursMod = Console.ReadLine();
                            Console.Write("Nouvelle durée: ");
                            if (decimal.TryParse(Console.ReadLine(), out decimal dureeCoursMod))
                            {
                                Console.Write("Nouvelle intensité: ");
                                if (int.TryParse(Console.ReadLine(), out int intensiteCoursMod))
                                {
                                    Console.Write("Nouvelle difficulté: ");
                                    string difficulteCoursMod = Console.ReadLine();
                                    admin.ModifierCours(idCoursMod, nomCoursMod, 
                                                       descCoursMod, dureeCoursMod, 
                                                       intensiteCoursMod, difficulteCoursMod);
                                }
                            }
                        }
                        Console.ReadKey();
                        break;
                    case "11":
                        Console.Write("ID du cours à supprimer: ");
                        if (int.TryParse(Console.ReadLine(), out int idCoursSup))
                            admin.SupprimerCours(idCoursSup);
                        Console.ReadKey();
                        break;
                    case "12":
                        admin.AfficherStatistiques();
                        Console.ReadKey();
                        break;
                    case "0":
                        inAdminMenu = false;
                        break;
                    default:
                        Console.WriteLine("[✗] Choix invalide");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Méthode: Menu pour les membres
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
                        Console.WriteLine("[✗] Choix invalide");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}