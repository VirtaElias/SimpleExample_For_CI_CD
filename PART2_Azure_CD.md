# Osa 2: Automaattinen julkaisu GitHub Actions -työkalulla

## Tavoite

Tässä tehtävässä automatisoit sovelluksen julkaisun Azureen käyttämällä **GitHub Actions** CI/CD-putkea. Kun pushaat koodin GitHubiin, sovellus rakentuu ja julkaistaan automaattisesti Azureen.

## Esivalmistelut

Varmista, että sinulla on:

- ✅ GitHub-tili
- ✅ Sovellus julkaistu Azureen (Osa 1 tehty)
- ✅ Git asennettuna
- ✅ Azure-tilisi toimii

## Mitä teemme?

```
Git Push → GitHub → GitHub Actions → Rakennus → Testaus → Azure Deployment
```

Kun pushaat muutoksen GitHubiin:
1. GitHub Actions havaitsee muutoksen
2. Rakentaa sovelluksen
3. Julkaisee automaattisesti Azureen
4. Ei tarvitse tehdä manuaalista deploymenttia enää!

---

## Vaihe 1: Luo GitHub Repository

Tehtävä tehdään classroomin kautta, joten sinulla on olemassa jo repository tälle. 

Jos classroom repon alle ei voi tehdä actioneita, niin siinä tapauksessa tee repo sinun omalle github tilille. Alla ohjeet sille, jos classroom ei toimi. Voit myös puskea koodit käyttämällä esim Visual Studion graafista käyttöliittymää.

### 1.1 Luo uusi repository GitHubissa

1. Avaa [GitHub.com](https://github.com)
2. Kirjaudu sisään
3. Klikkaa **+** (oikeassa yläkulmassa) → **New repository**
4. Täytä tiedot:
   - **Repository name**: `SimpleExample`
   - **Description**: `ASP.NET Core Web API with Azure deployment`
   - **Visibility**: Public tai Private (valitse itse)
   - **❌ ÄLÄ** valitse "Add a README file"
   - **❌ ÄLÄ** valitse .gitignore
   - **❌ ÄLÄ** valitse license
5. Klikkaa **Create repository**

### 1.2 Tallenna repository URL

GitHub näyttää sivun jossa on ohjeita. Kopioi **HTTPS URL**, esim:
```
https://github.com/KÄYTTÄJÄ/SimpleExample.git
```

---

## Vaihe 2: Alusta Git projektiisi

### 2.1 Avaa PowerShell projektin juuressa

```powershell
cd C:\Users\KÄYTTÄJÄ\source\repos\SimpleExample
```

### 2.2 Alusta Git (jos ei ole jo)

```bash
git init
```

### 2.3 Lisää .gitignore

Luo tiedosto `.gitignore` projektin juureen:

```gitignore
## Ignore Visual Studio temporary files
.vs/
bin/
obj/
*.user
*.suo

## Build results
[Dd]ebug/
[Rr]elease/
publish/
*.zip

## NuGet
packages/
*.nupkg

## Others
*.log
.vscode/
```

Tallenna tiedosto.

### 2.4 Lisää tiedostot Gittiin

```bash
git add .
git status
```

Tarkista että `bin/`, `obj/` ja `publish/` **EIVÄT** näy listalla!

### 2.5 Tee ensimmäinen commit

```bash
git commit -m "Initial commit - ASP.NET Core Web API"
```

### 2.6 Yhdistä GitHubiin

```bash
git branch -M main
git remote add origin https://github.com/KÄYTTÄJÄ/SimpleExample.git
git push -u origin main
```

**Jos kysyy tunnuksia:**
- Username: GitHub-käyttäjätunnuksesi
- Password: **Personal Access Token** (EI tavallinen salasana!)

### 2.7 Luo Personal Access Token (jos tarvitaan)

Jos Git pyytää salasanaa:

1. GitHub → **Settings** (oikeasta yläkulmasta)
2. **Developer settings** (alas vasemmalla)
3. **Personal access tokens** → **Tokens (classic)**
4. **Generate new token** → **Generate new token (classic)**
5. Anna nimi: `SimpleExample Deployment`
6. Valitse scope: ☑️ **repo** (kaikki)
7. Klikkaa **Generate token**
8. **KOPIOI TOKEN** heti (näkyy vain kerran!)
9. Käytä tokenia salasanan tilalla kun Git kysyy

### 2.8 Tarkista GitHub

Avaa repositorysi GitHubissa - pitäisi näkyä kaikki tiedostot!

---

## Vaihe 3: Hanki Azure Publish Profile

### 3.1 Lataa Publish Profile Azure Portalista

1. Avaa [Azure Portal](https://portal.azure.com)
2. Navigoi **App Service** -instanssiisi
3. Klikkaa **Get publish profile** (ylävalikosta)
4. Tiedosto `APPNAME.PublishSettings` latautuu

### 3.2 Avaa tiedosto tekstieditorilla

Avaa ladattu `.PublishSettings` -tiedosto Notepadilla tai VS Codella.

**Näyttää tältä:**
```xml
<publishData>
  <publishProfile profileName="APPNAME - Web Deploy" 
    publishUrl="APPNAME.scm.azurewebsites.net:443" 
    msdeploySite="APPNAME" 
    userName="$APPNAME" 
    userPWD="PITKÄ_SALASANA_TÄHÄN" 
    ...
  </publishProfile>
</publishData>
```

**KOPIOI KOKO TIEDOSTON SISÄLTÖ** (käytämme tätä GitHub Secretsissa).

---

## Vaihe 4: Lisää GitHub Secrets

### 4.1 Avaa repository Settings

1. GitHub repository → **Settings** (ylävalikosta)
2. Vasemmalta **Secrets and variables** → **Actions**
3. Klikkaa **New repository secret**

### 4.2 Luo AZURE_WEBAPP_PUBLISH_PROFILE Secret

1. **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
2. **Secret**: Liitä **koko** Publish Profile XML-sisältö
3. Klikkaa **Add secret**

### 4.3 Luo AZURE_WEBAPP_NAME Secret (valinnainen mutta suositeltava)

1. Klikkaa **New repository secret**
2. **Name**: `AZURE_WEBAPP_NAME`
3. **Secret**: App Servicesi nimi (esim. `simpleexample-app`)
4. Klikkaa **Add secret**

**Tarkista:** Sinulla pitäisi nyt olla 2 secretia:
- ✅ AZURE_WEBAPP_PUBLISH_PROFILE
- ✅ AZURE_WEBAPP_NAME

---

## Vaihe 5: Luo GitHub Actions Workflow

### 5.1 Luo .github/workflows -kansio

Projektin juuressa luo kansiorakenne:
```
.github/
  workflows/
    azure-deploy.yml
```

### 5.2 Luo azure-deploy.yml

Luo tiedosto `.github/workflows/azure-deploy.yml` ja kopioi sisältö:

```yaml
name: Deploy to Azure

# Workflow käynnistyy kun:
# 1. Pushaat main-branchiin
# 2. Käynnistät manuaalisesti (workflow_dispatch)
on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  DOTNET_VERSION: '9.0.x'              # .NET version
  AZURE_WEBAPP_NAME: 'SINUN-APP-SERVICE-NIMI'  # Vaihda tähän oma nimesi!

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    name: Build and Deploy to Azure
    
    steps:
    # 1. Checkout koodi GitHubista
    - name: Checkout code
      uses: actions/checkout@v4
    
    # 2. Asenna .NET
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    # 3. Restore dependencies
    - name: Restore dependencies
      run: dotnet restore
    
    # 4. Build
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    # 5. Publish
    - name: Publish
      run: dotnet publish SimpleExample.API/SimpleExample.API.csproj -c Release -o ./publish
    
    # 6. Deploy to Azure
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v3
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

**TÄRKEÄÄ:** Vaihda rivi 13:
```yaml
AZURE_WEBAPP_NAME: 'SINUN-APP-SERVICE-NIMI'
```

Vaihda `'SINUN-APP-SERVICE-NIMI'` omaan App Service -nimeesi!

### 5.3 Tarkista YAML-tiedosto

Varmista että:
- ✅ Sisennykset ovat oikein (käytä välilyöntejä, EI tabia)
- ✅ App Service -nimi on oikein
- ✅ .NET versio on `9.0.x`
- ✅ Projektin polku on oikein: `SimpleExample.API/SimpleExample.API.csproj`

---

## Vaihe 6: Pushaa Workflow GitHubiin

### 6.1 Lisää ja committaa

```bash
git add .github/workflows/azure-deploy.yml
git add .gitignore
git commit -m "Add GitHub Actions workflow for Azure deployment"
git push
```

### 6.2 Seuraa Workflown etenemistä

1. Avaa GitHub repository
2. Klikkaa **Actions** (ylävalikosta)
3. Näet workflow "Deploy to Azure" käynnistyneen
4. Klikkaa workflowta nähdäksesi yksityiskohdat

**Workflow vaiheet:**
- ⏳ Checkout code
- ⏳ Setup .NET
- ⏳ Restore dependencies
- ⏳ Build
- ⏳ Publish
- ⏳ Deploy to Azure Web App

Odota että kaikki vaiheet näyttävät ✅ (vihreä).

**Kesto:** Noin 2-4 minuuttia.

---

## Vaihe 7: Testaa automaattinen deployment

### 7.1 Tee pieni muutos koodiin

Avaa `SimpleExample.API/Controllers/UsersController.cs` ja lisää kommentti:

```csharp
/// <summary>
/// Get all users - Updated via GitHub Actions!
/// </summary>
[HttpGet]
public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
{
    ...
}
```

### 7.2 Pushaa muutos

```bash
git add .
git commit -m "Update API documentation"
git push
```

### 7.3 Seuraa GitHubissa

1. GitHub → **Actions**
2. Uusi workflow käynnistyy automaattisesti!
3. Seuraa että deployment onnistuu

### 7.4 Tarkista Azuresta

1. Avaa `https://SINUN-APP.azurewebsites.net/swagger`
2. Tarkista että päivitys näkyy
3. Testaa GET /api/users

**Odota 1-2 minuuttia** deploymentti jälkeen että muutos näkyy.

---

## Vaihe 8: Testaa manuaalinen käynnistys

### 8.1 Manuaalinen workflow

1. GitHub → **Actions**
2. Valitse vasemmalta **Deploy to Azure**
3. Klikkaa **Run workflow** (oikealla)
4. Valitse **Branch: main**
5. Klikkaa **Run workflow**

Tämä on hyödyllinen kun haluat julkaista ilman push-tapahtumaa.

---

## Vaihe 9: Dokumentoi

### Ota kuvakaappaukset ja tallenna ne `Pictures` -kansioon:

Varmista että `Pictures` -kansio on olemassa projektin juuressa.

**Tallenna seuraavat kuvakaappaukset:**

1. `11_GitHub_Repository.png` - GitHub repository pääsivu (näyttää tiedostot ja .github-kansio)
2. `12_GitHub_Secrets.png` - GitHub Secrets -sivu (näyttää että secretit on asetettu, **piilota arvot!**)
3. `13_GitHub_Actions.png` - GitHub Actions -sivu (lista workfloweista)
4. `14_Workflow_Run.png` - Yksittäinen workflow run (näyttää kaikki vaiheet vihreänä)
5. `15_Workflow_Log.png` - Workflow log (yksityiskohtainen loki jostakin vaiheesta)
6. `16_Swagger_Updated.png` - Swagger UI Azuressa (päivitetyn sovelluksen näyttö)

### Tallenna tiedostot:

Seuraavat tiedostot ovat jo repositoryssä, jotka olet tehnyt aiemmissa vaiheessa:
- `.github/workflows/azure-deploy.yml`
- `.gitignore`


---

## Vianmääritys (Troubleshooting)

### Ongelma: "Error: No such file or directory"

**Ratkaisu:** Tarkista että projektin polku on oikein workflowssa:
```yaml
run: dotnet publish SimpleExample.API/SimpleExample.API.csproj ...
```

### Ongelma: "Authentication failed"

**Ratkaisu:**
1. Lataa uusi Publish Profile Azure Portalista
2. Päivitä GitHub Secret: `AZURE_WEBAPP_PUBLISH_PROFILE`
3. Kokeile uudestaan

### Ongelma: "Resource not found"

**Ratkaisu:**
- Tarkista että `AZURE_WEBAPP_NAME` on oikein
- Varmista että App Service on olemassa Azuressa

### Ongelma: Workflow ei käynnisty

**Ratkaisu:**
1. Tarkista että workflow-tiedosto on `.github/workflows/` -kansiossa
2. Tiedoston nimi päättyy `.yml`
3. YAML syntaksi on oikein (sisennykset!)
4. Pushaa uudestaan: `git push`

### Ongelma: Build epäonnistuu

**Ratkaisu:**
```bash
# Testaa lokaalisti ensin
dotnet restore
dotnet build --configuration Release
dotnet publish SimpleExample.API/SimpleExample.API.csproj -c Release -o ./publish
```

Jos toimii lokaalisti, pitäisi toimia GitHub Actionsissa.

### Katso workflow lokeja:

1. GitHub → **Actions**
2. Klikkaa epäonnistunutta workflowta
3. Klikkaa punaista vaihetta
4. Lue virheviesti

---

## Palautettavat materiaalit

**1. GitHub Repository:**
- ✅ Repository URL (julkinen tai anna opettajalle pääsy)
- ✅ `.github/workflows/azure-deploy.yml` tiedosto näkyy repositoryssä

**2. Pictures-kansio kuvakaappauksilla:**

Varmista että `Pictures` -kansiossa on seuraavat kuvat:
- ✅ `11_GitHub_Repository.png` - GitHub repository pääsivu
- ✅ `12_GitHub_Secrets.png` - GitHub Secrets -sivu (arvot piilossa!)
- ✅ `13_GitHub_Actions.png` - GitHub Actions -sivu (workflow lista)
- ✅ `14_Workflow_Run.png` - Onnistunut workflow run (kaikki vaiheet vihreät)
- ✅ `15_Workflow_Log.png` - Workflow log (yksityiskohdat)
- ✅ `16_Swagger_Updated.png` - Swagger UI (päivitetty sovellus)


---

## Arviointikriteerit

### Erinomainen (5)
- GitHub Actions workflow toimii virheettömästi
- Automaattinen deployment toimii push-tapahtumalla
- Kaikki vaaditut kuvakaappaukset mukana
- Sovellus päivittyy Azuressa automaattisesti
- Bonus-parannukset toteutettu

### Hyvä (4)
- Workflow toimii
- Deployment onnistuu
- Kaikki kuvakaappaukset mukana

### Tyydyttävä (3)
- Workflow luotu ja toimii
- Vähintään 1 onnistunut automaattinen deployment
- Peruskuvakaappaukset mukana

### Välttävä (2)
- Workflow luotu mutta ei toimi täysin
- Yritystä näkyy

### Hylätty (0-1)
- Workflow ei toimi
- Ei todisteta automaattista deploymenttia

---

## Hyödyllisiä linkkejä

- [GitHub Actions dokumentaatio](https://docs.github.com/en/actions)
- [Azure Web Apps Deploy action](https://github.com/Azure/webapps-deploy)
- [YAML syntaksi](https://yaml.org/spec/1.2/spec.html)
- [Workflow syntax for GitHub Actions](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [GitHub Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)

---

## Yhteenveto

Olet nyt luonut automaattisen CI/CD-putken:

```
📝 Tee muutos koodiin
    ↓
💾 Git commit & push
    ↓
🚀 GitHub Actions havaitsee
    ↓
🔨 Rakennus ja testaus
    ↓
☁️ Automaattinen julkaisu Azureen
    ↓
✅ Sovellus päivittyy tuotannossa
```

**Ei enää manuaalista deploymenttia!** 

Kun pushaat muutoksen GitHubiin, sovellus päivittyy automaattisesti Azureen 2-4 minuutissa.

---

**Onnea tehtävän tekemiseen! **

Jos kohtaat ongelmia, tarkista vianmääritys-osio tai kysy apua.
