---
layout: default
title: Xiletrade
lang: fr-FR
description: Outil de superposition, de vérification des prix et d'aide pour la série Path Of Exile
strings:
  maintained: projet est maintenu par
  generated: Cette page a été générée par
  download: Télécharger
  repository: Voir sur GitHub
  footer: Service gratuit nécessitant une maintenance continue pour fonctionner correctement.
  issue: et utilise
  latest: dernière release
  support: Contribuer
---
{% include screenshot.html %}
## Vérifier le prix de votre article en un clic

*Trouvez instantanément la meilleure gamme de prix.*  
*Configurez les comportements à la volée.*  
*Jouez plus facilement, tradez plus rapidement !*  

## {% include youtube.svg %} &nbsp; [Vérification](https://youtu.be/4mP3uOsr8oc) &nbsp; [Échanges](https://youtu.be/6yuLZXTho-A) &nbsp; [Paramètres](https://youtu.be/libdIjrNM-8)<br>

Fonctionne efficacement avec peu de ressources CPU.  
Utilise max ***250 Mo de RAM*** sans pics d'allocation de mémoire.  
***AUCUNE DONNÉE*** écrite en arrière-plan.  

* * *

# {% include pokeball.svg %} Principales fonctionnalitées

- **Vérificateur de prix** rapide pour tous les articles à l'aide du raccourci défini ***CTRL+D*** *par défaut*.
- Ouvrez les **Paramètres** avec ***CTRL+R*** pour modifier les comportements de vérification des prix.
- **Échange d'articles en vrac** pour des échanges rapides via POE.
- **Addons** pour aider à vérifier le prix des articles et récupérer plus d'info sur ces derniers.
	- Liaison logicielle avec **poe.prices**, **poeninja**, **poewiki** et **poedb**.
- Vérifie les **nouvelles mises à jour** et actualise les **filtres** personnalisés/officiels au démarrage.
- Fonctionnalités respectant la règle **d'action unique** de la [Politique d'application tierces](https://www.pathofexile.com/developer/docs#policy) :
	- Interagissez avec la **console de discussion** en envoyant des messages prédéfinis à l'aide de **raccourci** 
tel que le déplacement rapide vers la **cachette** ou les actions **d'échange**.
	- Liaison de la **molette de la souris** avec le **clic gauche** à la demande.
- Utilitaire **Regex manager**.
	- Gérez une liste définie d'**expressions régulières**.
	- Coller automatiquement dans la barre de surbrillance.  

<img align="center" src="https://github.com/user-attachments/assets/1a3229fe-9f61-4c18-b4de-98e2ee026ace">
<br>

## {% include install.svg %} Installation et configuration

1. [**Téléchargez**]({% include download.html %}) la dernière version et **extrayez** l'archive **`.7zip`** dans le répertoire souhaité.
Xiletrade est une **application portable** et n'installe rien de plus au lancement.
2. **Clic droit** sur l'icône de la barre d'état système pour configurer ou fermer le programme.
3. Sélectionnez la **version du jeu**, la **langue** et la **ligue** dans la fenêtre de démarrage ou de paramètres.
4. **Passez la souris** sur chaque fonctionnalitée jusqu'à ce qu'une info-bulle apparaisse pour obtenir une explication détaillée.  
<br>
<img src="https://github.com/user-attachments/assets/2aa8b83a-9144-4b56-8d79-1808aac0d486">
<br>

* * *
> # {% include mouse.svg %} Comment ça marche
>
> Exécutez votre jeu en mode **plein écran fenêtré** ou **sans bordure** pour qu'il fonctionne correctement.
> Assurez-vous que la **langue** et la **ligue** de Xiletrade correspondent aux paramètres de jeu correspondants.
> Une fois Xiletrade lancé et configuré, vous pouvez maintenant **vérifier les prix** en suivant ce processus :
> 1. Placez votre souris **sur un objet** dans le jeu et appuyez sur ***CTRL+D*** *(par défaut)*
> 2. Il copiera les **les informations sur les objets** du jeu et ouvrira la fenêtre de Xiletrade.
> 3. La fenêtre affichera les **prix estimés** si la recherche renvoie des résultats correspondants.
> 4. Les prix affichés sont basés sur les **sites Web d'échange officiels** [PoE 1](https://www.pathofexile.com/trade/search/) et [PoE 2](https://www.pathofexile.com/trade2/search/poe2/).
<br>

### {% include chip.svg %} Comportements personnalisables

* **Faites glisser la fenêtre principale** où vous le souhaitez sur votre écran.
* Recherchez par **valeurs actuelles** des objets ou par **valeur minimale** dans la plage de niveaux.
* Modifiez les valeurs numériques avec la **molette de la souris** (min/max)
* Maintenez la touche **CTRL** ou **SHIFT** enfoncée pour obtenir des valeurs décimales.
* Ajustez l'**opacité** et la **fermeture automatique** lorsque la fenêtre perd le focus.
* Cliquez sur l'icône **en haut à gauche** de la fenêtre principale.
* **Mettez en surbrillance** les mods **chers** et **dangereux** dans les cartes (fichier de config).
* **Collez automatiquement** les échanges chuchotés dans le jeu à partir de sites Web externes.

```
Conçu pour respecter les règles fixées par le site officiel du commerce afin d'éviter 
les abus avec une récupération de données limitée et des demandes au fil du temps.
```
* * *

## {% include finger.svg %} FAQ

<p class="accordion"><b>L'application prend-elle en charge toutes les langues ?</b></p>
<div class="panel">
<b>Oui</b>, elle a été conçue pour prendre en charge toutes les langues du client Path of Exile.
</div>

<p class="accordion"><b>Est-il sûr de télécharger l'application à partir d'un autre site Web ?</b></p>
<div class="panel">
Pour des raisons de sécurité évidentes, il est fortement recommandé de télécharger Xiletrade à partir du référentiel principal ou du site Web github.io associé.
</div>

<p class="accordion"><b>Je sais que l'utilisation d'une application tierce peut entraîner un bannissement d'un jeu.<br>Est-ce le cas pour Path of Exile avec Xiletrade ?</b></p>
<div class="panel">
<b>Non</b>, Xiletrade est sorti pour la première fois en 2020 et tant que vous utilisez la version publiée sur ce site, vous ne risquerez jamais d'être banni tant que la politique de GGG autorise l'utilisation d'outils comme celui-ci.
<br>A noter que l'éditeur du jeu ne garantira jamais un outil tiers.
Il appartient au mainteneur du projet de s'assurer que l'outil proposé respecte les règles.
<br>Je vous invite à lire les <a target="_blank" rel="noopener noreferrer" href="https://www.pathofexile.com/developer/docs#policy">conditions d'utilisation</a> afin de croiser les informations.
</div>

<p class="accordion"><b>Pourquoi le logiciel est-il gratuit ?</b></p>
<div class="panel">
Xiletrade est un <b>logiciel autonome</b> et n'est pas destiné à être utilisé à des fins lucratives en intégrant des publicités comme il est d'usage. Cependant, il est possible de faire des <a target="_blank" rel="noopener noreferrer" href="{{ site.github.paypal_url }}">dons</a> à votre discrétion. Les contributions sont appréciées si vous aimez le projet et merci pour cela 😊.
</div>