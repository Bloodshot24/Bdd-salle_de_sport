CREATE DATABASE salle_sport;
USE salle_sport;

CREATE TABLE Coach (ID_Coach INT PRIMARY KEY AUTO_INCREMENT,
Nom VARCHAR(121) NOT NULL,
Prenom VARCHAR(121) NOT NULL,
Tel VARCHAR(30),
MDP VARCHAR(50) NOT NULL,
Formation VARCHAR(50),
Specialite VARCHAR(50));

CREATE TABLE Admin (ID_Admin INT PRIMARY KEY AUTO_INCREMENT,
Nom VARCHAR(121) NOT NULL,
Prenom VARCHAR(121) NOT NULL,
Nv_Privilege INT NOT NULL,
MDP VARCHAR(50) NOT NULL);

CREATE TABLE Membre (ID_Membre INT PRIMARY KEY AUTO_INCREMENT,
Nom VARCHAR(121) NOT NULL,
Prenom VARCHAR(121) NOT NULL,
Adresse VARCHAR(150),
Tel VARCHAR(30),
Mail VARCHAR(150),
Date_Inscription DATE NOT NULL,
Validite_Inscription BOOLEAN NOT NULL DEFAULT TRUE,
MDP VARCHAR(50) NOT NULL,
ID_Admin INT,
CONSTRAINT fk_membre_admin FOREIGN KEY (ID_Admin) REFERENCES Admin(ID_Admin)); 

CREATE TABLE Salle (ID_Salle INT PRIMARY KEY AUTO_INCREMENT,
Nom_Salle VARCHAR(50) NOT NULL,
Localisation VARCHAR(150),
Capacite INT NOT NULL); 

CREATE TABLE Cours (ID_Cours INT PRIMARY KEY AUTO_INCREMENT,
Nom_Cours VARCHAR(50) NOT NULL,
Description VARCHAR(300),
Duree_h_min DECIMAL(4,2) NOT NULL,
Intensite INT,
Difficulte VARCHAR(20),
Capacite INT);

CREATE TABLE Planning (
ID_Planning INT PRIMARY KEY AUTO_INCREMENT,
ID_Coach INT NOT NULL,
ID_Cours INT NOT NULL,
ID_Salle INT NOT NULL,
Date_H_Debut DATETIME NOT NULL,
Date_H_Fin DATETIME NOT NULL,
CONSTRAINT fk_planning_coach FOREIGN KEY (ID_Coach) REFERENCES Coach(ID_Coach),
CONSTRAINT fk_planning_cours FOREIGN KEY (ID_Cours) REFERENCES Cours(ID_Cours),
CONSTRAINT fk_planning_salle FOREIGN KEY (ID_Salle) REFERENCES Salle(ID_Salle));

CREATE TABLE Inscription (ID_Membre INT NOT NULL,
ID_Cours INT NOT NULL,
Date_Inscription DATETIME,
PRIMARY KEY (ID_Membre, ID_Cours),
CONSTRAINT fk_inscription_membre FOREIGN KEY (ID_Membre) REFERENCES Membre(ID_Membre),
CONSTRAINT fk_inscription_cours FOREIGN KEY (ID_Cours) REFERENCES Cours(ID_Cours));

INSERT INTO Admin (Nom, Prenom, Nv_Privilege, MDP) VALUES
('Martin',  'Julie',  1, 'admin001'),
('Durand',  'Paul',   2, 'admin002'),
('Lefevre', 'Sarah',  1, 'admin003');

INSERT INTO Coach (Nom, Prenom, Tel, MDP, Formation, Specialite) VALUES
('Bernard',  'Lucas',  '0611223344', 'coach001', 'BPJEPS',      'Musculation'),
('Robert',   'Emma',   '0622334455', 'coach002', 'STAPS',       'Cardio'),
('Moreau',   'Hugo',   '0633445566', 'coach003', 'BPJEPS',      'CrossFit'),
('Laurent',  'Chloe',  '0644556677', 'coach004', 'STAPS',       'Yoga'),
('Roux',     'Nathan', '0655667788', 'coach005', 'BPJEPS',      'Boxe');

INSERT INTO Salle (Nom_Salle, Localisation, Capacite) VALUES
('Salle Muscu 1', 'RDC - Aile Nord', 30),
('Salle Cardio',  'RDC - Aile Sud',  25),
('Studio Cours',  '1er étage',       40);

INSERT INTO Cours (Nom_Cours, Description, Duree_h_min, Intensite, Difficulte, Capacite) VALUES
('BodyPump',   'Cours de renforcement musculaire', 1.00, 4, 'Moyen',   25),
('HIIT',       'Entraînement fractionné intense',  0.75, 5, 'Difficile',20),
('Yoga',       'Séance de relaxation et étirements',1.00,2,'Facile',   30),
('Pilates',    'Travail du gainage et posture',    1.00,3,'Moyen',    20),
('Cycling',    'Cardio sur vélo en musique',       0.75,4,'Moyen',    25);

INSERT INTO Membre
  (Nom, Prenom, Adresse, Tel, Mail, Date_Inscription, Validite_Inscription, MDP, ID_Admin)
VALUES
  ('Martin',   'Lucas',    '10 rue Victor Hugo, Paris',       '0611223344', 'lucas.martin@test.com',   '2025-01-10', 1, 'mdp001', 1),
  ('Dubois',   'Emma',     '5 av. de la République, Lyon',    '0622334455', 'emma.dubois@test.com',    '2025-01-11', 1, 'mdp002', 1),
  ('Bernard',  'Léo',      '2 rue Nationale, Lille',          '0633445566', 'leo.bernard@test.com',    '2025-01-12', 1, 'mdp003', 1),
  ('Robert',   'Chloé',    '8 bd Foch, Bordeaux',             '0644556677', 'chloe.robert@test.com',   '2025-01-13', 1, 'mdp004', 1),
  ('Richard',  'Noah',     '3 rue des Lilas, Toulouse',       '0655667788', 'noah.richard@test.com',   '2025-01-14', 1, 'mdp005', 1),
  ('Petit',    'Inès',     '11 rue des Écoles, Nice',         '0666778899', 'ines.petit@test.com',     '2025-01-15', 1, 'mdp006', 1),
  ('Durand',   'Hugo',     '15 rue Garibaldi, Lyon',          '0677889900', 'hugo.durand@test.com',    '2025-01-16', 1, 'mdp007', 1),
  ('Leroy',    'Manon',    '20 rue de Metz, Toulouse',        '0612345678', 'manon.leroy@test.com',    '2025-01-17', 1, 'mdp008', 1),
  ('Moreau',   'Louis',    '7 rue Alsace-Lorraine, Rennes',   '0623456789', 'louis.moreau@test.com',   '2025-01-18', 1, 'mdp009', 1),
  ('Simon',    'Camille',  '4 rue du Port, Nantes',           '0634567890', 'camille.simon@test.com',  '2025-01-19', 1, 'mdp010', 1),
  ('Laurent',  'Nathan',   '6 rue Colbert, Tours',            '0645678901', 'nathan.laurent@test.com', '2025-01-20', 1, 'mdp011', 1),
  ('Lefebvre', 'Sarah',    '9 rue des Arts, Montpellier',     '0656789012', 'sarah.lefebvre@test.com', '2025-01-21', 1, 'mdp012', 1),
  ('Morel',    'Tom',      '2 rue Royale, Paris',             '0667890123', 'tom.morel@test.com',      '2025-01-22', 1, 'mdp013', 1),
  ('Girard',   'Léa',      '18 rue Thiers, Dijon',            '0678901234', 'lea.girard@test.com',     '2025-01-23', 1, 'mdp014', 1),
  ('Roux',     'Mathis',   '12 rue d Italie, Marseille',      '0610987654', 'mathis.roux@test.com',    '2025-01-24', 1, 'mdp015', 1),
  ('Blanc',    'Zoé',      '14 rue de la Paix, Reims',        '0621098765', 'zoe.blanc@test.com',      '2025-01-25', 1, 'mdp016', 1),
  ('Guerin',   'Adam',     '22 rue du Rhône, Genève',         '0632109876', 'adam.guerin@test.com',    '2025-01-26', 1, 'mdp017', 1),
  ('Muller',   'Julie',    '30 rue des Vosges, Strasbourg',   '0643210987', 'julie.muller@test.com',   '2025-01-27', 1, 'mdp018', 1),
  ('Henry',    'Ethan',    '16 rue du Stade, Brest',          '0654321098', 'ethan.henry@test.com',    '2025-01-28', 1, 'mdp019', 1),
  ('Roussel',  'Anna',     '19 rue Lafayette, Paris',         '0665432109', 'anna.roussel@test.com',   '2025-01-29', 1, 'mdp020', 1),
  ('Nicolas',  'Jules',    '3 rue de Rome, Marseille',        '0676543210', 'jules.nicolas@test.com',  '2025-01-30', 1, 'mdp021', 1),
  ('Perrin',   'Lina',     '8 rue Alsace, Metz',              '0612340001', 'lina.perrin@test.com',    '2025-01-31', 1, 'mdp022', 1),
  ('Morin',    'Noé',      '5 rue du Centre, Angers',         '0623450002', 'noe.morin@test.com',      '2025-02-01', 1, 'mdp023', 1),
  ('Breton',   'Eva',      '27 rue des Prés, Caen',           '0634560003', 'eva.breton@test.com',     '2025-02-02', 1, 'mdp024', 1),
  ('Renaud',   'Yanis',    '6 rue du Parc, Clermont-Ferrand', '0645670004', 'yanis.renaud@test.com',   '2025-02-03', 1, 'mdp025', 1),
  ('Marchand', 'Alice',    '10 rue du Château, Orléans',      '0656780005', 'alice.marchand@test.com', '2025-02-04', 1, 'mdp026', 1),
  ('Bertrand', 'Lina',     '21 rue Victor Hugo, Lille',       '0667890006', 'lina.bertrand@test.com',  '2025-02-05', 1, 'mdp027', 1),
  ('Faure',    'Noah',     '4 rue des Fleurs, Nîmes',         '0678900007', 'noah.faure@test.com',     '2025-02-06', 1, 'mdp028', 1),
  ('Carpentier','Maël',    '13 rue des Sports, Grenoble',     '0612123434', 'mael.carpentier@test.com','2025-02-07', 1, 'mdp029', 1),
  ('Gaillard', 'Louna',    '9 rue de la Liberté, Perpignan',  '0623234545', 'louna.gaillard@test.com', '2025-02-08', 1, 'mdp030', 1);


INSERT INTO Planning (ID_Coach, ID_Cours, ID_Salle, Date_H_Debut, Date_H_Fin) VALUES
(1, 1, 1, '2025-03-01 09:00:00', '2025-03-01 10:00:00'),
(2, 2, 2, '2025-03-01 10:30:00', '2025-03-01 11:15:00'),
(3, 3, 3, '2025-03-01 18:00:00', '2025-03-01 19:00:00'),
(4, 4, 3, '2025-03-02 17:00:00', '2025-03-02 18:00:00'),
(5, 5, 2, '2025-03-02 19:00:00', '2025-03-02 19:45:00');

INSERT INTO Inscription (ID_Membre, ID_Cours, Date_Inscription) VALUES
(1, 1, '2025-02-10 10:00:00'),
(2, 1, '2025-02-10 10:05:00'),
(3, 2, '2025-02-11 11:00:00'),
(4, 2, '2025-02-11 11:10:00'),
(5, 3, '2025-02-12 12:00:00'),
(6, 3, '2025-02-12 12:05:00'),
(7, 4, '2025-02-13 18:00:00'),
(8, 5, '2025-02-14 19:00:00'),
(9, 5, '2025-02-14 19:05:00'),
(10,2, '2025-02-15 09:00:00');


