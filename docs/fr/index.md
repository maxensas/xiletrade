---
layout: default
title: Xiletrade
lang: fr-FR
minidesc: Vérificateur de prix pour Path of Exile
description: Un outil qui facilite la vérification des prix d’objets dans Path of Exile 1 et Path of Exile 2
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
## Vérifier le prix de votre article

<h2 class="keywords"><i>🔍 Trouve instantanément le meilleur prix.</i></h2>
<h2 class="keywords"><i>⚡ Configure les comportements à la volée.</i></h2>
<h2 class="keywords"><i>🎮 Joue plus facilement, échange plus vite !</i></h2>

#### {% include youtube.svg %} &nbsp; [Voir aperçu](https://youtu.be/NygCZvZyUX0)<br>

Faible utilisation du CPU.  
Utilise au maximum 250 Mo de RAM, sans pics.  
Aucune donnée écrite en arrière-plan.  

* * *
<br><br>

### {% include direction.svg %} Introduction rapide

<h2 class="keywords"><strong>Xiletrade propose une interface minimaliste pour une intégration optimale avec Path of Exile 1 et 2.</strong></h2>
<section class="intro">
  <p>Grâce à ses fonctionnalités, il améliorera votre qualité de vie pendant vos parties en ligues commerciales, garantissant une bonne expérience de jeu.</p>
  <p>En tant que logiciel open source, sa base de code reçoit une attention méticuleuse pour vous fournir l’outil le plus optimisé possible. 
  Vous trouverez ci-dessous la liste complète des comportements personnalisables concernant la vérification de prix des objets sur POE.</p>
</section>

### {% include pokeball.svg %} Principales fonctionnalitées

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

{% include regex.html %}
<br>

### {% include install.svg %} Installation et configuration

1. [**Téléchargez**]({% include download.html %}) la dernière version et **extrayez** l'archive **`.7zip`** dans le répertoire souhaité.
Xiletrade est une **application portable** et n'installe rien de plus au lancement.
2. **Clic droit** sur l'icône de la barre d'état système pour configurer ou fermer le programme.
3. Sélectionnez la **version du jeu**, la **langue** et la **ligue** dans la fenêtre de démarrage ou de paramètres.
4. **Passez la souris** sur chaque fonctionnalitée jusqu'à ce qu'une info-bulle apparaisse pour obtenir une explication détaillée.  
<br>
{% include settings.html %}
<br>

* * *
> ### {% include mouse.svg %} Comment ça marche
>
> Exécutez votre jeu en mode **plein écran fenêtré** ou **sans bordure** pour qu'il fonctionne correctement.
> Assurez-vous que la **langue** et la **ligue** de Xiletrade correspondent aux paramètres de jeu correspondants.
> Une fois Xiletrade lancé et configuré, vous pouvez maintenant **vérifier les prix** en suivant ce processus :
> 1. Placez votre souris **sur un objet** dans le jeu et appuyez sur ***CTRL+D*** *(par défaut)*
> 2. Il copiera les **les informations sur les objets** du jeu et ouvrira la fenêtre de Xiletrade.
> 3. La fenêtre affichera les **prix estimés** si la recherche renvoie des résultats correspondants.
> 4. Les prix affichés sont basés sur les **sites Web d'échange officiels** [PoE 1](https://www.pathofexile.com/trade/search/) et [PoE 2](https://www.pathofexile.com/trade2/search/poe2/).
<p class="accordion"><b>Consulter les détails concernant la vérification de prix d'objets</b></p>
<div class="panel"> N'oubliez pas que le jeu doit être au premier plan, sinon le raccourci clavier ne fonctionnera pas.
<br><br> Par exemple, si vous laissez la fenêtre Xiletrade ouverte, cliquez dessus (afin que Xiletrade ait le focus), puis essayez une nouvelle vérification de prix, cela ne fonctionnera jamais. En revanche, vous pouvez vérifier le prix de plusieurs objets consécutivement tant que vous ne cliquez pas sur la fenêtre Xiletrade.
<br><br> Chaque fois que PoE perd le focus, les raccourcis clavier sont automatiquement désenregistrés puis réenregistrés lorsque PoE retrouve le focus.
<br><br> Faites également attention à ne pas effectuer trop de vérifications de prix d'affilée afin d'éviter un blocage temporaire du site de commerce PoE. L'outil vous avertira si vous dépassez ses limites et vous protégera contre un blocage massif.
</div>

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

### {% include finger.svg %} FAQ

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