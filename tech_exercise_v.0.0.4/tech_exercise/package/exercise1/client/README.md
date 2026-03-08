# Stargate Client

Angular frontend for the **Stargate API** (Astronaut Career Tracking System). It lets you view people, add or edit people, and view or manage astronaut duties (rank, title, start/end dates). Built with **Angular 21** and **Node 24**.

---

## Prerequisites

- [Node.js](https://nodejs.org/) **24.x** or later (see `engines` in `package.json`)
- [npm](https://www.npmjs.com/) (comes with Node)

---

## Setup and run

### 1. Install dependencies

From the **client** folder:

```bash
cd tech_exercise_v.0.0.4/tech_exercise/package/exercise1/client
npm install
```

### 2. Start the Stargate API

The client talks to the Stargate API. Start it first (from the **api** folder):

```bash
cd ../api
dotnet run
```

The API runs at **`http://localhost:5204`** by default (see `api/Properties/launchSettings.json`). CORS is enabled for `http://localhost:4200`.

### 3. Start the Angular dev server

From the **client** folder:

```bash
ng serve
```

Or:

```bash
npm start
```

Open **`http://localhost:4200/`** in your browser. The app will reload when you change source files.

**Proxy:** The dev server uses `proxy.conf.json` to forward `/Person`, `/AstronautDuty`, and `/Rank` to the API. The app uses `environment.apiUrl` (e.g. `http://localhost:5204` in dev), so API calls work whether they go through the proxy or directly to that URL.

---

## Configuring the API URL

- **Development:** Edit `src/environments/environment.ts` and set `apiUrl` (default `http://localhost:5204`). Use the same port as your API.
- **Production:** Edit `src/environments/environment.prod.ts`; `apiUrl` is typically `''` so requests use the same origin as the deployed app.

The shared `apiBase` in `src/app/shared/api-base.ts` reads from the active environment.

---

## Building

From the **client** folder:

```bash
ng build
```

Or:

```bash
npm run build
```

- **Production (default):** Output goes to `dist/client/`. Uses `environment.prod.ts` (e.g. empty `apiUrl` for same-origin).
- **Development:** `ng build --configuration development` for a dev build with source maps and no prod optimizations.

---

## Running tests

Tests use [Karma](https://karma-runner.github.io) and [Jasmine](https://jasmine.github.io/).

### Run tests (with coverage)

From the **client** folder:

```bash
ng test
```

Or:

```bash
npm test
```

This runs tests in a browser (Chrome by default), watches for file changes, and generates a **code coverage** report. When the run finishes, the coverage report is under **`coverage/`** (e.g. `coverage/client/index.html`). Open that in a browser to see line/branch coverage.

### Run tests once (no watch)

To run tests a single time and exit (e.g. for CI):

```bash
ng test --no-watch
```

### Run tests without coverage

To skip coverage and speed up the run:

```bash
ng test --no-watch --code-coverage=false
```

(If your Angular CLI version uses a different flag, check `ng test --help`.)

---

## Project structure

| Path | Purpose |
|------|--------|
| `src/app/pages/` | Route components: view people, view person details, add/edit person |
| `src/app/components/` | Reusable UI (e.g. duty card) |
| `src/app/shared/` | Services (person, astronaut-duty, rank), models, `api-base.ts` |
| `src/app/app.routes.ts` | Route definitions |
| `src/environments/` | `environment.ts` (dev) and `environment.prod.ts` (prod) |
| `proxy.conf.json` | Dev-server proxy from `/Person`, `/AstronautDuty`, `/Rank` to the API |

**Main routes:**

- `/` or `/view-people` – list all people  
- `/add-person` – add a new person (optionally with astronaut duty)  
- `/edit-person/:name` – edit person and duties by name  
- `/view-person-details/:name` – view one person and their duties  

---

## Troubleshooting

- **API calls fail or 404:** Ensure the Stargate API is running at the URL in `environment.apiUrl` (default `http://localhost:5204`). If the API uses a different port, update `apiUrl` and, if you rely on the proxy, the `target` in `proxy.conf.json`.
- **CORS errors:** The API must allow `http://localhost:4200`. It’s configured in `api/Program.cs`; if you change the client port, update CORS there too.
- **`ng serve` fails:** Confirm Node 24+ (`node -v`) and run `npm install` again. Delete `node_modules` and run `npm install` if dependencies look broken.
- **Tests hang or don’t open browser:** Run `ng test --no-watch` once to see results in the terminal, or check `angular.json` for the Karma browser setting.

---

## Additional resources

- [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli)  
- [Angular Documentation](https://angular.dev)
