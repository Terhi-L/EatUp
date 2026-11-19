import { execFileSync } from 'node:child_process'

function run(command, args, env = {}) {
  execFileSync(command, args, { stdio: 'inherit', shell: true, env: { ...process.env, ...env } })
}

// Development, so the /api/dev endpoints the Profile screen uses are in the document too.
run('dotnet', ['build', 'backend/src/EatUp.Api', '-p:OpenApiGenerateDocuments=true'], { ASPNETCORE_ENVIRONMENT: 'Development' })

// openapi-typescript needs the TypeScript 5 compiler API, which TypeScript 7 no longer has; npx keeps it out of the project.
run('npx', ['-y', '-p', 'typescript@5.9.3', '-p', 'openapi-typescript@7.13.0', 'openapi-typescript', 'web/openapi.json', '-o', 'web/src/api-schema.d.ts'])