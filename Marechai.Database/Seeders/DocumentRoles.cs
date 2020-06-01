/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Marechai.Database.Models;

// ReSharper disable StringLiteralTypo

namespace Marechai.Database.Seeders
{
    public static class DocumentRoles
    {
        public static void Seed(MarechaiContext context)
        {
            List<DocumentRole> existingRoles             = context.DocumentRoles.ToList();
            List<DocumentRole> newDocumentRoles          = new List<DocumentRole>();
            int                updatedDocumentRolesCount = 0;

            foreach(DocumentRole role in new[]
            {
                new DocumentRole
                {
                    Id = "abr", Name = "Abridger", Enabled = true
                },
                new DocumentRole
                {
                    Id = "acp", Name = "Art copyist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "act", Name = "Actor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "adi", Name = "Art director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "adp", Name = "Adapter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "aft", Name = "Author of afterword, colophon, etc.", Enabled = true
                },
                new DocumentRole
                {
                    Id = "anl", Name = "Analyst", Enabled = true
                },
                new DocumentRole
                {
                    Id = "anm", Name = "Animator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ann", Name = "Annotator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ant", Name = "Bibliographic antecedent", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ape", Name = "Appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "apl", Name = "Appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "app", Name = "Applicant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "aqt", Name = "Author in quotations or text abstracts", Enabled = true
                },
                new DocumentRole
                {
                    Id = "arc", Name = "Architect", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ard", Name = "Artistic director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "arr", Name = "Arranger", Enabled = true
                },
                new DocumentRole
                {
                    Id = "art", Name = "Artist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "asg", Name = "Assignee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "asn", Name = "Associated name", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ato", Name = "Autographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "att", Name = "Attributed name", Enabled = true
                },
                new DocumentRole
                {
                    Id = "auc", Name = "Auctioneer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "aud", Name = "Author of dialog", Enabled = true
                },
                new DocumentRole
                {
                    Id = "aui", Name = "Author of introduction, etc.", Enabled = true
                },
                new DocumentRole
                {
                    Id = "aus", Name = "Screenwriter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "aut", Name = "Author", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bdd", Name = "Binding designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bjd", Name = "Bookjacket designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bkd", Name = "Book designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bkp", Name = "Book producer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "blw", Name = "Blurb writer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bnd", Name = "Binder", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bpd", Name = "Bookplate designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "brd", Name = "Broadcaster", Enabled = true
                },
                new DocumentRole
                {
                    Id = "brl", Name = "Braille embosser", Enabled = true
                },
                new DocumentRole
                {
                    Id = "bsl", Name = "Bookseller", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cas", Name = "Caster", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ccp", Name = "Conceptor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "chr", Name = "Choreographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "clb", Name = "Collaborator", Enabled = false
                },
                new DocumentRole
                {
                    Id = "cli", Name = "Client", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cll", Name = "Calligrapher", Enabled = true
                },
                new DocumentRole
                {
                    Id = "clr", Name = "Colorist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "clt", Name = "Collotyper", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cmm", Name = "Commentator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cmp", Name = "Composer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cmt", Name = "Compositor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cnd", Name = "Conductor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cng", Name = "Cinematographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cns", Name = "Censor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "coe", Name = "Contestant-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "col", Name = "Collector", Enabled = true
                },
                new DocumentRole
                {
                    Id = "com", Name = "Compiler", Enabled = true
                },
                new DocumentRole
                {
                    Id = "con", Name = "Conservator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cor", Name = "Collection registrar", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cos", Name = "Contestant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cot", Name = "Contestant-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cou", Name = "Court governed", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cov", Name = "Cover designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cpc", Name = "Copyright claimant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cpe", Name = "Complainant-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cph", Name = "Copyright holder", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cpl", Name = "Complainant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cpt", Name = "Complainant-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cre", Name = "Creator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "crp", Name = "Correspondent", Enabled = true
                },
                new DocumentRole
                {
                    Id = "crr", Name = "Corrector", Enabled = true
                },
                new DocumentRole
                {
                    Id = "crt", Name = "Court reporter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "csl", Name = "Consultant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "csp", Name = "Consultant to a project", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cst", Name = "Costume designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ctb", Name = "Contributor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cte", Name = "Contestee-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ctg", Name = "Cartographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ctr", Name = "Contractor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cts", Name = "Contestee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ctt", Name = "Contestee-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cur", Name = "Curator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "cwt", Name = "Commentator for written text", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dbp", Name = "Distribution place", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dfd", Name = "Defendant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dfe", Name = "Defendant-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dft", Name = "Defendant-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dgg", Name = "Degree granting institution", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dgs", Name = "Degree supervisor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dis", Name = "Dissertant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dln", Name = "Delineator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dnc", Name = "Dancer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dnr", Name = "Donor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dpc", Name = "Depicted", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dpt", Name = "Depositor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "drm", Name = "Draftsman", Enabled = true
                },
                new DocumentRole
                {
                    Id = "drt", Name = "Director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dsr", Name = "Designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dst", Name = "Distributor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dtc", Name = "Data contributor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dte", Name = "Dedicatee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dtm", Name = "Data manager", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dto", Name = "Dedicator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "dub", Name = "Dubious author", Enabled = true
                },
                new DocumentRole
                {
                    Id = "edc", Name = "Editor of compilation", Enabled = true
                },
                new DocumentRole
                {
                    Id = "edm", Name = "Editor of moving image work", Enabled = true
                },
                new DocumentRole
                {
                    Id = "edt", Name = "Editor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "egr", Name = "Engraver", Enabled = true
                },
                new DocumentRole
                {
                    Id = "elg", Name = "Electrician", Enabled = true
                },
                new DocumentRole
                {
                    Id = "elt", Name = "Electrotyper", Enabled = true
                },
                new DocumentRole
                {
                    Id = "eng", Name = "Engineer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "enj", Name = "Enacting jurisdiction", Enabled = true
                },
                new DocumentRole
                {
                    Id = "etr", Name = "Etcher", Enabled = true
                },
                new DocumentRole
                {
                    Id = "evp", Name = "Event place", Enabled = true
                },
                new DocumentRole
                {
                    Id = "exp", Name = "Expert", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fac", Name = "Facsimilist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fds", Name = "Film distributor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fld", Name = "Field director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "flm", Name = "Film editor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fmd", Name = "Film director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fmk", Name = "Filmmaker", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fmo", Name = "Former owner", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fmp", Name = "Film producer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fnd", Name = "Funder", Enabled = true
                },
                new DocumentRole
                {
                    Id = "fpy", Name = "First party", Enabled = true
                },
                new DocumentRole
                {
                    Id = "frg", Name = "Forger", Enabled = true
                },
                new DocumentRole
                {
                    Id = "gis", Name = "Geographic information specialist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "grt", Name = "Graphic technician", Enabled = false
                },
                new DocumentRole
                {
                    Id = "his", Name = "Host institution", Enabled = true
                },
                new DocumentRole
                {
                    Id = "hnr", Name = "Honoree", Enabled = true
                },
                new DocumentRole
                {
                    Id = "hst", Name = "Host", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ill", Name = "Illustrator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ilu", Name = "Illuminator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ins", Name = "Inscriber", Enabled = true
                },
                new DocumentRole
                {
                    Id = "inv", Name = "Inventor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "isb", Name = "Issuing body", Enabled = true
                },
                new DocumentRole
                {
                    Id = "itr", Name = "Instrumentalist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ive", Name = "Interviewee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ivr", Name = "Interviewer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "jud", Name = "Judge", Enabled = true
                },
                new DocumentRole
                {
                    Id = "jug", Name = "Jurisdiction governed", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lbr", Name = "Laboratory", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lbt", Name = "Librettist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ldr", Name = "Laboratory director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "led", Name = "Lead", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lee", Name = "Libelee-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lel", Name = "Libelee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "len", Name = "Lender", Enabled = true
                },
                new DocumentRole
                {
                    Id = "let", Name = "Libelee-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lgd", Name = "Lighting designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lie", Name = "Libelant-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lil", Name = "Libelant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lit", Name = "Libelant-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lsa", Name = "Landscape architect", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lse", Name = "Licensee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lso", Name = "Licensor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ltg", Name = "Lithographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "lyr", Name = "Lyricist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mcp", Name = "Music copyist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mdc", Name = "Metadata contact", Enabled = true
                },
                new DocumentRole
                {
                    Id = "med", Name = "Medium", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mfp", Name = "Manufacture place", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mfr", Name = "Manufacturer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mod", Name = "Moderator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mon", Name = "Monitor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mrb", Name = "Marbler", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mrk", Name = "Markup editor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "msd", Name = "Musical director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mte", Name = "Metal-engraver", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mtk", Name = "Minute taker", Enabled = true
                },
                new DocumentRole
                {
                    Id = "mus", Name = "Musician", Enabled = true
                },
                new DocumentRole
                {
                    Id = "nrt", Name = "Narrator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "opn", Name = "Opponent", Enabled = true
                },
                new DocumentRole
                {
                    Id = "org", Name = "Originator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "orm", Name = "Organizer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "osp", Name = "Onscreen presenter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "oth", Name = "Other", Enabled = true
                },
                new DocumentRole
                {
                    Id = "own", Name = "Owner", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pan", Name = "Panelist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pat", Name = "Patron", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pbd", Name = "Publishing director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pbl", Name = "Publisher", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pdr", Name = "Project director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pfr", Name = "Proofreader", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pht", Name = "Photographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "plt", Name = "Platemaker", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pma", Name = "Permitting agency", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pmn", Name = "Production manager", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pop", Name = "Printer of plates", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ppm", Name = "Papermaker", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ppt", Name = "Puppeteer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pra", Name = "Praeses", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prc", Name = "Process contact", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prd", Name = "Production personnel", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pre", Name = "Presenter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prf", Name = "Performer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prg", Name = "Programmer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prm", Name = "Printmaker", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prn", Name = "Production company", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pro", Name = "Producer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prp", Name = "Production place", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prs", Name = "Production designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prt", Name = "Printer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "prv", Name = "Provider", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pta", Name = "Patent applicant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pte", Name = "Plaintiff-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ptf", Name = "Plaintiff", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pth", Name = "Patent holder", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ptt", Name = "Plaintiff-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "pup", Name = "Publication place", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rbr", Name = "Rubricator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rcd", Name = "Recordist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rce", Name = "Recording engineer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rcp", Name = "Addressee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rdd", Name = "Radio director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "red", Name = "Redaktor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ren", Name = "Renderer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "res", Name = "Researcher", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rev", Name = "Reviewer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rpc", Name = "Radio producer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rps", Name = "Repository", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rpt", Name = "Reporter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rpy", Name = "Responsible party", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rse", Name = "Respondent-appellee", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rsg", Name = "Restager", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rsp", Name = "Respondent", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rsr", Name = "Restorationist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rst", Name = "Respondent-appellant", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rth", Name = "Research team head", Enabled = true
                },
                new DocumentRole
                {
                    Id = "rtm", Name = "Research team member", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sad", Name = "Scientific advisor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sce", Name = "Scenarist", Enabled = true
                },
                new DocumentRole
                {
                    Id = "scl", Name = "Sculptor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "scr", Name = "Scribe", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sds", Name = "Sound designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sec", Name = "Secretary", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sgd", Name = "Stage director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sgn", Name = "Signer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sht", Name = "Supporting host", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sll", Name = "Seller", Enabled = true
                },
                new DocumentRole
                {
                    Id = "sng", Name = "Singer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "spk", Name = "Speaker", Enabled = true
                },
                new DocumentRole
                {
                    Id = "spn", Name = "Sponsor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "spy", Name = "Second party", Enabled = true
                },
                new DocumentRole
                {
                    Id = "srv", Name = "Surveyor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "std", Name = "Set designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "stg", Name = "Setting", Enabled = true
                },
                new DocumentRole
                {
                    Id = "stl", Name = "Storyteller", Enabled = true
                },
                new DocumentRole
                {
                    Id = "stm", Name = "Stage manager", Enabled = true
                },
                new DocumentRole
                {
                    Id = "stn", Name = "Standards body", Enabled = true
                },
                new DocumentRole
                {
                    Id = "str", Name = "Stereotyper", Enabled = true
                },
                new DocumentRole
                {
                    Id = "tcd", Name = "Technical director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "tch", Name = "Teacher", Enabled = true
                },
                new DocumentRole
                {
                    Id = "ths", Name = "Thesis advisor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "tld", Name = "Television director", Enabled = true
                },
                new DocumentRole
                {
                    Id = "tlp", Name = "Television producer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "trc", Name = "Transcriber", Enabled = true
                },
                new DocumentRole
                {
                    Id = "trl", Name = "Translator", Enabled = true
                },
                new DocumentRole
                {
                    Id = "tyd", Name = "Type designer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "tyg", Name = "Typographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "uvp", Name = "University place", Enabled = true
                },
                new DocumentRole
                {
                    Id = "vac", Name = "Voice actor", Enabled = true
                },
                new DocumentRole
                {
                    Id = "vdg", Name = "Videographer", Enabled = true
                },
                new DocumentRole
                {
                    Id = "voc", Name = "Vocalist", Enabled = false
                },
                new DocumentRole
                {
                    Id = "wac", Name = "Writer of added commentary", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wal", Name = "Writer of added lyrics", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wam", Name = "Writer of accompanying material", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wat", Name = "Writer of added text", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wdc", Name = "Woodcutter", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wde", Name = "Wood engraver", Enabled = true
                },
                new DocumentRole
                {
                    Id = "win", Name = "Writer of introduction", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wit", Name = "Witness", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wpr", Name = "Writer of preface", Enabled = true
                },
                new DocumentRole
                {
                    Id = "wst", Name = "Writer of supplementary textual content", Enabled = true
                }
            })
            {
                DocumentRole existingRole = existingRoles.FirstOrDefault(r => r.Id == role.Id);

                if(existingRole is null)
                    newDocumentRoles.Add(role);
                else
                {
                    bool changed = false;

                    if(role.Name != existingRole.Name)
                    {
                        existingRole.Name = role.Name;
                        changed           = true;
                    }

                    if(role.Enabled != existingRole.Enabled)
                    {
                        existingRole.Enabled = role.Enabled;
                        changed              = true;
                    }

                    if(changed)
                        updatedDocumentRolesCount++;
                }
            }

            context.DocumentRoles.AddRange(newDocumentRoles);

            Console.WriteLine("{0} document roles will be added.", newDocumentRoles.Count);
            Console.WriteLine("{0} document roles will be updated.", updatedDocumentRolesCount);
        }
    }
}