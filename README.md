# 🚀 Smart Task Flow

> Une application web intelligente de gestion des tâches qui aide les utilisateurs à mieux organiser leur quotidien selon l'énergie, le temps et la priorité réelle.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-11.0-239120?style=flat-square&logo=csharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat-square&logo=microsoftsqlserver)
![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=flat-square&logo=html5&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=flat-square&logo=javascript&logoColor=black)


## 🎯 À propos

**Smart Task Flow** est une application de gestion de tâches intelligente qui se distingue des applications traditionnelles par son système de recommandation basé sur trois facteurs clés :

- ⏱ **Temps disponible** - Combien de temps avez-vous ?
- 🔥 **Niveau d'énergie** - Êtes-vous en forme ou fatigué ?
- ⚠ **Priorité réelle** - Qu'est-ce qui est vraiment urgent ?

L'application propose automatiquement la meilleure tâche à accomplir selon votre état actuel, maximisant ainsi votre productivité.

## ✨ Fonctionnalités

### 👤 Pour les Utilisateurs

- ✅ Création et gestion complète des tâches (CRUD)
- 🎯 Catégorisation des tâches (Travail, Études, Personnel, Sport, Maison)
- ⚡ Attribution de niveau d'énergie (Faible, Moyen, Élevé)
- ⏰ Gestion des deadlines et durées estimées
- 🤖 **Recommandation intelligente** - "Que faire maintenant ?"
- 📊 Statistiques personnelles de productivité
- 📅 Vue des tâches du jour et en retard
- 🔐 Authentification sécurisée avec JWT

### 👨‍💼 Pour les Administrateurs

- 📊 Dashboard avec statistiques globales en temps réel
- 👥 Gestion complète des utilisateurs
- 🔍 Recherche et filtrage avancés
- 🚫 Blocage/déblocage de comptes
- 🗑️ Suppression d'utilisateurs
- 📈 Analytics et rapports détaillés
- 📋 Visualisation de toutes les tâches du système
- 📉 Suivi du taux de complétion global

## 🛠 Technologies

### Backend
- **Framework** : ASP.NET Core 8.0 Web API
- **Langage** : C# 11.0
- **ORM** : Entity Framework Core 8.0
- **Base de données** : SQL Server 2022
- **Authentification** : JWT (JSON Web Tokens)
- **Hashing** : ASP.NET Core Identity (PasswordHasher)

### Frontend
- **HTML5** - Structure
- **CSS3** - Styling (Variables CSS, Flexbox, Grid)
- **JavaScript (ES6+)** - Logique et communication API
- **Font Awesome 6.4** - Icônes

### Architecture
- **Pattern** : RESTful API
- **Architecture** : MVC (Model-View-Controller)
- **Sécurité** : CORS, JWT Authentication, Password Hashing

## 🏗 Architecture
```
SmartTaskFlow/
├── Controllers/              # Contrôleurs API
│   ├── AccountController.cs  # Authentification et profils
│   ├── AdminController.cs    # Gestion administrateur
│   └── TaskController.cs     # Gestion des tâches
├── Models/                   # Modèles de données
│   ├── User.cs              # Modèle utilisateur
│   ├── Task.cs              # Modèle tâche
│   └── Category.cs          # Modèle catégorie
├── Data/                     # Contexte de base de données
│   └── ApplicationDbContext.cs
├── DTOs/                     # Data Transfer Objects
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   ├── CreateTaskDto.cs
│   ├── UpdateTaskDto.cs
│   ├── UpdateProfileDto.cs
│   └── ChangePasswordDto.cs
├── Services/                 # Services métier
│   └── TokenService.cs      # Génération de tokens JWT
├── Frontend/                 # Interface utilisateur
│   └── admin-dashboard.html # Dashboard administrateur
├── Program.cs                # Point d'entrée de l'application
└── appsettings.json         # Configuration
```

## 📦 Installation

### Prérequis

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou supérieur
- [SQL Server 2022](https://www.microsoft.com/sql-server/sql-server-downloads) ou SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommandé) ou VS Code
- [Git](https://git-scm.com/)

### Étapes d'installation

1. **Cloner le repository**
```bash
git clone https://github.com/votre-username/smart-task-flow.git
cd smart-task-flow
```

2. **Restaurer les packages NuGet**
```bash
dotnet restore
```

Ou dans Visual Studio Package Manager Console :
```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.AspNetCore.Authentication.JwtBearer
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

3. **Configurer la base de données**

Modifiez `appsettings.json` avec votre chaîne de connexion SQL Server :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=VOTRE_SERVEUR;Database=SmartTaskFlowDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Exemples de chaînes de connexion :

- **LocalDB** : `Server=(localdb)\\mssqllocaldb;Database=SmartTaskFlowDB;Trusted_Connection=True;`
- **SQL Express** : `Server=localhost\\SQLEXPRESS;Database=SmartTaskFlowDB;Trusted_Connection=True;`
- **SQL Server** : `Server=localhost;Database=SmartTaskFlowDB;Trusted_Connection=True;`

4. **Appliquer les migrations**
```bash
# Ligne de commande
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Ou dans Package Manager Console (Visual Studio) :
```powershell
Add-Migration InitialCreate
Update-Database
```

5. **Lancer l'application**
```bash
dotnet run
```

Ou dans Visual Studio : Appuyez sur **F5**

L'API sera disponible sur : `https://localhost:7XXX` (le port sera affiché dans la console)

## ⚙️ Configuration

### appsettings.json complet
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartTaskFlowDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "SuperSecretKey123456789VeryLongAndSecure!@#$%",
    "Issuer": "SmartTaskFlow",
    "Audience": "SmartTaskFlowUsers",
    "ExpireMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Configuration du Frontend

Dans `Frontend/admin-dashboard.html`, modifiez la ligne 631 avec votre port :
```javascript
const API_URL = 'https://localhost:7123/api'; // Remplacez 7123 par votre port
```


### Workflow utilisateur typique

1. **Inscription** → Créer un compte utilisateur via `/api/account/register`
2. **Connexion** → S'authentifier et recevoir un token JWT
3. **Créer des tâches** → Ajouter des tâches avec catégorie, priorité, niveau d'énergie
4. **Utiliser la recommandation** → "J'ai 30 minutes et un niveau d'énergie moyen, que faire ?"
5. **Suivre la progression** → Consulter ses statistiques personnelles

### Workflow administrateur

1. **Connexion admin** → Se connecter au dashboard avec le compte admin
2. **Consulter les stats** → Vue globale : utilisateurs, tâches, taux de complétion
3. **Gérer les utilisateurs** → Voir détails, bloquer, ou supprimer des comptes
4. **Analyser les données** → Rapports sur les tâches en retard, les utilisateurs actifs, etc.

``
```

## 🔒 Sécurité

- ✅ **Authentification JWT** - Tokens sécurisés avec expiration
- ✅ **Hashing des mots de passe** - ASP.NET Core Identity PasswordHasher
- ✅ **Autorisation par rôle** - Admin vs User
- ✅ **Validation des données** - Data Annotations sur les DTOs
- ✅ **CORS configuré** - Protection contre les requêtes non autorisées
- ✅ **HTTPS** - Communication sécurisée





```


## 🤝 Contribuer

Les contributions sont les bienvenues ! Voici comment procéder :

1. Forkez le projet
2. Créez une branche pour votre fonctionnalité (`git checkout -b feature/AmazingFeature`)
3. Committez vos changements (`git commit -m 'Add some AmazingFeature'`)
4. Pushez vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrez une Pull Request

### Guidelines

- Respectez les conventions de nommage C#
- Ajoutez des tests pour les nouvelles fonctionnalités
- Mettez à jour la documentation si nécessaire
- Suivez les principes SOLID

## 🐛 Problèmes connus

- Le frontend nécessite CORS activé pour fonctionner
- Les tokens JWT expirent après 24h (configurable)
- La recherche est sensible à la casse (amélioration à venir)



## 👨‍💻 Auteur

**Cherni Oumaima**
- GitHub: [@cherni2003](https://github.com/cherni2003)
- Email: cherni.oumaima2003@gmail.com

⭐ Si ce projet vous a été utile, n'hésitez pas à lui donner une étoile !
 
