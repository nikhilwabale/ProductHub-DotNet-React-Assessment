# Install the React UI

Open a terminal in this folder and run the standard command:

```bash
npm i
```

Then start the UI:

```bash
npm run dev
```

The project-level `.npmrc` uses the official npm registry and sets
`replace-registry-host=never`. This keeps the public package URLs recorded in
`package-lock.json` even when a machine-level npm registry override exists.
