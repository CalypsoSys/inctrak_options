1. Use [docs/inctrak_production_runbook.md](/home/joe/dotnet/inctrak_options/docs/inctrak_production_runbook.md:1) as the source of truth.
2. Build the API image from `shared.inctrak.com/` into `C:\transfer\inctrak-api-latest.tar.gz`.
3. Copy the image tarball, `config.yaml`, `docker/inctrak/docker-compose.yml`, `scripts/inctrak/compose-inctrak.sh`, and `render-config-env` to `/srv/stacks/inctrak/api`.
4. On the host, use `./scripts/compose-inctrak.sh ...` instead of the old demo/shared docker directories.
