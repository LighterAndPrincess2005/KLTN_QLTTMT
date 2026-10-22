# Self-service profiles and internal reading

## Profile registration

- Account details are refreshed from `GET /api/tai-khoan/toi` when the registration form opens.
- Father/mother/guardian: only emergency contact name and telephone are populated from the signed-in member. Child name and date of birth remain separate.
- Self: identity fields use available account data; emergency contact name and telephone are also populated. Fields remain editable.
- Signup now sends and persists `NgaySinh` on `ThanhVien`; `GET /api/tai-khoan/toi` returns it after a fresh login. Apply `Database/006_NgaySinhThanhVien.sql` to existing databases. Historical dates previously kept only in browser session storage cannot be recovered on a different device; never infer the member's birthday from a child's profile. A newly saved self profile may supply the member's previously missing date explicitly.
- New accounts are active immediately. Creating a new student creates an active student and an effective full-right representative link to its creator, without a separate staff approval step. Previously pending members can activate through creating their own new student profile.
- Existing read-only representative permissions are not promoted. Blocked/inactive records are not activated. Age, self-representation age limits, capacity, schedule and payment checks remain enforced. Entry assessment is advisory and no longer blocks enrollment.
- `Database/005_BoChoDuyetHoSoCu.sql` explicitly migrates existing pending, effective full/registration-right links and pending active-account profiles. Review and back up first. The script is transactional/idempotent, preserves read-only/expired/blocked records and writes before-state snapshots to the audit log.
- Saving a student selects its returned ID and reloads the profile list.
- Source changes require running the newly built API, not merely refreshing the browser.

## Reading

- `/bai-viet`: seven editorial entries, search and category selection.
- `/bai-viet/:slug`: original educational copy, images, article contents, exercise, related entries and compact references.
- Primary Method reading cards and artwork/story buttons remain within this website. Source references are deliberately secondary and collapsed.
- Native links preserve the development preview query parameter and support reload/back navigation. No iframe or automatic external redirect is used.

## Verification

- Seven article routes plus search, reload, not-found handling and 375/768/1440 px widths checked with browser automation.
- Parent contact autofill and immediate new-profile selection checked in browser preview.
- HTTP integration suite checks self-service activation, profile contact response and existing authorization boundaries against a temporary SQL Server LocalDB test database.
