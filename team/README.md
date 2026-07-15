# LPG Management System - Team Knowledge Base

AI-augmented development knowledge base for the LPG Cylinder Distribution ERP system.

## Project Structure

```
team/
├── README.md                    # This file
├── docs/
│   └── business-requirements.md # LPG system business rules and requirements
├── process/
│   └── development-workflow.md  # How we plan, build, and verify
├── templates/
│   ├── feature-spec.md          # Template for feature specifications
│   └── feature-spec-example.md  # Completed example spec
├── standards/                   # Code and design standards
│   ├── architecture.md          # Clean architecture rules
│   ├── api.md                   # REST API design standards
│   ├── coding.md                # General coding style
│   ├── database.md              # EF Core and SQL Server rules
│   └── angular.md               # Frontend standards
└── agents/                      # AI agent role definitions
    ├── architect.md             # Architecture decisions and guidance
    ├── backend.md               # Backend implementation patterns
    ├── frontend.md              # Angular frontend patterns
    ├── mobile.md                # Flutter mobile patterns
    ├── devops.md                # Deployment and operations
    ├── qa.md                    # Testing and quality assurance
    └── security.md              # Security review and posture
```

## How to Use

### Starting a Feature
1. Copy `templates/feature-spec.md`
2. Fill in the spec with requirements
3. Reference `docs/business-requirements.md` for business rules
4. Give the spec to the AI agent as the task description

### During Development
1. Follow `process/development-workflow.md`
2. Reference relevant `standards/*.md` files
3. Use `agents/*.md` for role-specific guidance

### Key Documents
- **Business Requirements**: `docs/business-requirements.md`
- **Architecture**: `team/standards/architecture.md`
- **Coding Standards**: `team/standards/coding.md`
- **API Standards**: `team/standards/api.md`
- **Database Standards**: `team/standards/database.md`

## Agent Roles

| Agent | Responsibility |
|-------|----------------|
| Architect | Architecture decisions, design reviews, pattern enforcement |
| Backend | API endpoints, services, domain logic, database operations |
| Frontend | Angular UI, components, state management |
| Mobile | Flutter app, offline support, field operations |
| DevOps | Deployment, CI/CD, infrastructure, monitoring |
| QA | Testing, validation, verification |
| Security | Security review, auth, secrets management |

## Core Business Concept

**Gas and Cylinders are separate assets with independent lifecycles.**

- Cylinder: reusable, returnable asset (tracks ownership, movement, deposits)
- Gas: consumable product (tracks inventory, consumption, refills)

Every transaction must distinguish between cylinder and gas operations.

## Writing for LLMs

- Be prescriptive: "Use X. Never do Y."
- Show Good/Avoid examples
- Use real paths and names from this repo
- State the why for non-obvious rules
- Keep files scoped to one topic
