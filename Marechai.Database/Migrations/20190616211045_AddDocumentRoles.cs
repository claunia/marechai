﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDocumentRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("DocumentRoles", table => new
            {
                Id      = table.Column<string>("char(3)"),
                Name    = table.Column<string>(nullable: true),
                Enabled = table.Column<bool>(nullable: false, defaultValue: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DocumentRoles", x => x.Id);
            });

            migrationBuilder.InsertData("DocumentRoles", new[]
            {
                "Id", "Enabled", "Name"
            }, new object[,]
            {
                {
                    "abr", true, "Abridger"
                },
                {
                    "orm", true, "Organizer"
                },
                {
                    "osp", true, "Onscreen presenter"
                },
                {
                    "oth", true, "Other"
                },
                {
                    "own", true, "Owner"
                },
                {
                    "pan", true, "Panelist"
                },
                {
                    "pat", true, "Patron"
                },
                {
                    "pbd", true, "Publishing director"
                },
                {
                    "pbl", true, "Publisher"
                },
                {
                    "pdr", true, "Project director"
                },
                {
                    "pfr", true, "Proofreader"
                },
                {
                    "pht", true, "Photographer"
                },
                {
                    "plt", true, "Platemaker"
                },
                {
                    "pma", true, "Permitting agency"
                },
                {
                    "pmn", true, "Production manager"
                },
                {
                    "org", true, "Originator"
                },
                {
                    "pop", true, "Printer of plates"
                },
                {
                    "ppt", true, "Puppeteer"
                },
                {
                    "pra", true, "Praeses"
                },
                {
                    "prc", true, "Process contact"
                },
                {
                    "prd", true, "Production personnel"
                },
                {
                    "pre", true, "Presenter"
                },
                {
                    "prf", true, "Performer"
                },
                {
                    "prg", true, "Programmer"
                },
                {
                    "prm", true, "Printmaker"
                },
                {
                    "prn", true, "Production company"
                },
                {
                    "pro", true, "Producer"
                },
                {
                    "prp", true, "Production place"
                },
                {
                    "prs", true, "Production designer"
                },
                {
                    "prt", true, "Printer"
                },
                {
                    "prv", true, "Provider"
                },
                {
                    "ppm", true, "Papermaker"
                },
                {
                    "pta", true, "Patent applicant"
                },
                {
                    "opn", true, "Opponent"
                },
                {
                    "mus", true, "Musician"
                },
                {
                    "jug", true, "Jurisdiction governed"
                },
                {
                    "lbr", true, "Laboratory"
                },
                {
                    "lbt", true, "Librettist"
                },
                {
                    "ldr", true, "Laboratory director"
                },
                {
                    "led", true, "Lead"
                },
                {
                    "lee", true, "Libelee-appellee"
                },
                {
                    "lel", true, "Libelee"
                },
                {
                    "len", true, "Lender"
                },
                {
                    "let", true, "Libelee-appellant"
                },
                {
                    "lgd", true, "Lighting designer"
                },
                {
                    "lie", true, "Libelant-appellee"
                },
                {
                    "lil", true, "Libelant"
                },
                {
                    "lit", true, "Libelant-appellant"
                },
                {
                    "lsa", true, "Landscape architect"
                },
                {
                    "nrt", true, "Narrator"
                },
                {
                    "lse", true, "Licensee"
                },
                {
                    "ltg", true, "Lithographer"
                },
                {
                    "lyr", true, "Lyricist"
                },
                {
                    "mcp", true, "Music copyist"
                },
                {
                    "mdc", true, "Metadata contact"
                },
                {
                    "med", true, "Medium"
                },
                {
                    "mfp", true, "Manufacture place"
                },
                {
                    "mfr", true, "Manufacturer"
                },
                {
                    "mod", true, "Moderator"
                },
                {
                    "mon", true, "Monitor"
                },
                {
                    "mrb", true, "Marbler"
                },
                {
                    "mrk", true, "Markup editor"
                },
                {
                    "msd", true, "Musical director"
                },
                {
                    "mte", true, "Metal-engraver"
                },
                {
                    "mtk", true, "Minute taker"
                },
                {
                    "lso", true, "Licensor"
                },
                {
                    "jud", true, "Judge"
                },
                {
                    "pte", true, "Plaintiff-appellee"
                },
                {
                    "pth", true, "Patent holder"
                },
                {
                    "spn", true, "Sponsor"
                },
                {
                    "spy", true, "Second party"
                },
                {
                    "srv", true, "Surveyor"
                },
                {
                    "std", true, "Set designer"
                },
                {
                    "stg", true, "Setting"
                },
                {
                    "stl", true, "Storyteller"
                },
                {
                    "stm", true, "Stage manager"
                },
                {
                    "stn", true, "Standards body"
                },
                {
                    "str", true, "Stereotyper"
                },
                {
                    "tcd", true, "Technical director"
                },
                {
                    "tch", true, "Teacher"
                },
                {
                    "ths", true, "Thesis advisor"
                },
                {
                    "tld", true, "Television director"
                },
                {
                    "tlp", true, "Television producer"
                },
                {
                    "spk", true, "Speaker"
                },
                {
                    "trc", true, "Transcriber"
                },
                {
                    "tyd", true, "Type designer"
                },
                {
                    "tyg", true, "Typographer"
                },
                {
                    "uvp", true, "University place"
                },
                {
                    "vac", true, "Voice actor"
                },
                {
                    "vdg", true, "Videographer"
                },
                {
                    "voc", false, "Vocalist"
                },
                {
                    "wac", true, "Writer of added commentary"
                },
                {
                    "wal", true, "Writer of added lyrics"
                },
                {
                    "wam", true, "Writer of accompanying material"
                },
                {
                    "wat", true, "Writer of added text"
                },
                {
                    "wdc", true, "Woodcutter"
                },
                {
                    "wde", true, "Wood engraver"
                },
                {
                    "win", true, "Writer of introduction"
                },
                {
                    "wit", true, "Witness"
                },
                {
                    "trl", true, "Translator"
                },
                {
                    "ptf", true, "Plaintiff"
                },
                {
                    "sng", true, "Singer"
                },
                {
                    "sht", true, "Supporting host"
                },
                {
                    "ptt", true, "Plaintiff-appellant"
                },
                {
                    "pup", true, "Publication place"
                },
                {
                    "rbr", true, "Rubricator"
                },
                {
                    "rcd", true, "Recordist"
                },
                {
                    "rce", true, "Recording engineer"
                },
                {
                    "rcp", true, "Addressee"
                },
                {
                    "rdd", true, "Radio director"
                },
                {
                    "red", true, "Redaktor"
                },
                {
                    "ren", true, "Renderer"
                },
                {
                    "res", true, "Researcher"
                },
                {
                    "rev", true, "Reviewer"
                },
                {
                    "rpc", true, "Radio producer"
                },
                {
                    "rps", true, "Repository"
                },
                {
                    "rpt", true, "Reporter"
                },
                {
                    "sll", true, "Seller"
                },
                {
                    "rpy", true, "Responsible party"
                },
                {
                    "rsg", true, "Restager"
                },
                {
                    "rsp", true, "Respondent"
                },
                {
                    "rsr", true, "Restorationist"
                },
                {
                    "rst", true, "Respondent-appellant"
                },
                {
                    "rth", true, "Research team head"
                },
                {
                    "rtm", true, "Research team member"
                },
                {
                    "sad", true, "Scientific advisor"
                },
                {
                    "sce", true, "Scenarist"
                },
                {
                    "scl", true, "Sculptor"
                },
                {
                    "scr", true, "Scribe"
                },
                {
                    "sds", true, "Sound designer"
                },
                {
                    "sec", true, "Secretary"
                },
                {
                    "sgd", true, "Stage director"
                },
                {
                    "sgn", true, "Signer"
                },
                {
                    "rse", true, "Respondent-appellee"
                },
                {
                    "wpr", true, "Writer of preface"
                },
                {
                    "ivr", true, "Interviewer"
                },
                {
                    "itr", true, "Instrumentalist"
                },
                {
                    "brl", true, "Braille embosser"
                },
                {
                    "bsl", true, "Bookseller"
                },
                {
                    "cas", true, "Caster"
                },
                {
                    "ccp", true, "Conceptor"
                },
                {
                    "chr", true, "Choreographer"
                },
                {
                    "clb", false, "Collaborator"
                },
                {
                    "cli", true, "Client"
                },
                {
                    "cll", true, "Calligrapher"
                },
                {
                    "clr", true, "Colorist"
                },
                {
                    "clt", true, "Collotyper"
                },
                {
                    "cmm", true, "Commentator"
                },
                {
                    "cmp", true, "Composer"
                },
                {
                    "cmt", true, "Compositor"
                },
                {
                    "cnd", true, "Conductor"
                },
                {
                    "brd", true, "Broadcaster"
                },
                {
                    "cng", true, "Cinematographer"
                },
                {
                    "coe", true, "Contestant-appellee"
                },
                {
                    "col", true, "Collector"
                },
                {
                    "com", true, "Compiler"
                },
                {
                    "con", true, "Conservator"
                },
                {
                    "cor", true, "Collection registrar"
                },
                {
                    "cos", true, "Contestant"
                },
                {
                    "cot", true, "Contestant-appellant"
                },
                {
                    "cou", true, "Court governed"
                },
                {
                    "cov", true, "Cover designer"
                },
                {
                    "cpc", true, "Copyright claimant"
                },
                {
                    "cpe", true, "Complainant-appellee"
                },
                {
                    "cph", true, "Copyright holder"
                },
                {
                    "cpl", true, "Complainant"
                },
                {
                    "cpt", true, "Complainant-appellant"
                },
                {
                    "cns", true, "Censor"
                },
                {
                    "cre", true, "Creator"
                },
                {
                    "bpd", true, "Bookplate designer"
                },
                {
                    "blw", true, "Blurb writer"
                },
                {
                    "acp", true, "Art copyist"
                },
                {
                    "act", true, "Actor"
                },
                {
                    "adi", true, "Art director"
                },
                {
                    "adp", true, "Adapter"
                },
                {
                    "aft", true, "Author of afterword, colophon, etc."
                },
                {
                    "anl", true, "Analyst"
                },
                {
                    "anm", true, "Animator"
                },
                {
                    "ann", true, "Annotator"
                },
                {
                    "ant", true, "Bibliographic antecedent"
                },
                {
                    "ape", true, "Appellee"
                },
                {
                    "apl", true, "Appellant"
                },
                {
                    "app", true, "Applicant"
                },
                {
                    "aqt", true, "Author in quotations or text abstracts"
                },
                {
                    "arc", true, "Architect"
                },
                {
                    "bnd", true, "Binder"
                },
                {
                    "ard", true, "Artistic director"
                },
                {
                    "art", true, "Artist"
                },
                {
                    "asg", true, "Assignee"
                },
                {
                    "asn", true, "Associated name"
                },
                {
                    "ato", true, "Autographer"
                },
                {
                    "att", true, "Attributed name"
                },
                {
                    "auc", true, "Auctioneer"
                },
                {
                    "aud", true, "Author of dialog"
                },
                {
                    "aui", true, "Author of introduction, etc."
                },
                {
                    "aus", true, "Screenwriter"
                },
                {
                    "aut", true, "Author"
                },
                {
                    "bdd", true, "Binding designer"
                },
                {
                    "bjd", true, "Bookjacket designer"
                },
                {
                    "bkd", true, "Book designer"
                },
                {
                    "bkp", true, "Book producer"
                },
                {
                    "arr", true, "Arranger"
                },
                {
                    "ive", true, "Interviewee"
                },
                {
                    "crp", true, "Correspondent"
                },
                {
                    "crt", true, "Court reporter"
                },
                {
                    "edt", true, "Editor"
                },
                {
                    "egr", true, "Engraver"
                },
                {
                    "elg", true, "Electrician"
                },
                {
                    "elt", true, "Electrotyper"
                },
                {
                    "eng", true, "Engineer"
                },
                {
                    "enj", true, "Enacting jurisdiction"
                },
                {
                    "etr", true, "Etcher"
                },
                {
                    "evp", true, "Event place"
                },
                {
                    "exp", true, "Expert"
                },
                {
                    "fac", true, "Facsimilist"
                },
                {
                    "fds", true, "Film distributor"
                },
                {
                    "fld", true, "Field director"
                },
                {
                    "flm", true, "Film editor"
                },
                {
                    "fmd", true, "Film director"
                },
                {
                    "edm", true, "Editor of moving image work"
                },
                {
                    "fmk", true, "Filmmaker"
                },
                {
                    "fmp", true, "Film producer"
                },
                {
                    "fnd", true, "Funder"
                },
                {
                    "fpy", true, "First party"
                },
                {
                    "frg", true, "Forger"
                },
                {
                    "gis", true, "Geographic information specialist"
                },
                {
                    "grt", false, "Graphic technician"
                },
                {
                    "his", true, "Host institution"
                },
                {
                    "hnr", true, "Honoree"
                },
                {
                    "hst", true, "Host"
                },
                {
                    "ill", true, "Illustrator"
                },
                {
                    "ilu", true, "Illuminator"
                },
                {
                    "ins", true, "Inscriber"
                },
                {
                    "inv", true, "Inventor"
                },
                {
                    "isb", true, "Issuing body"
                },
                {
                    "fmo", true, "Former owner"
                },
                {
                    "crr", true, "Corrector"
                },
                {
                    "edc", true, "Editor of compilation"
                },
                {
                    "dto", true, "Dedicator"
                },
                {
                    "csl", true, "Consultant"
                },
                {
                    "csp", true, "Consultant to a project"
                },
                {
                    "cst", true, "Costume designer"
                },
                {
                    "ctb", true, "Contributor"
                },
                {
                    "cte", true, "Contestee-appellee"
                },
                {
                    "ctg", true, "Cartographer"
                },
                {
                    "ctr", true, "Contractor"
                },
                {
                    "cts", true, "Contestee"
                },
                {
                    "ctt", true, "Contestee-appellant"
                },
                {
                    "cur", true, "Curator"
                },
                {
                    "cwt", true, "Commentator for written text"
                },
                {
                    "dbp", true, "Distribution place"
                },
                {
                    "dfd", true, "Defendant"
                },
                {
                    "dfe", true, "Defendant-appellee"
                },
                {
                    "dub", true, "Dubious author"
                },
                {
                    "dft", true, "Defendant-appellant"
                },
                {
                    "dgs", true, "Degree supervisor"
                },
                {
                    "dis", true, "Dissertant"
                },
                {
                    "dln", true, "Delineator"
                },
                {
                    "dnc", true, "Dancer"
                },
                {
                    "dnr", true, "Donor"
                },
                {
                    "dpc", true, "Depicted"
                },
                {
                    "dpt", true, "Depositor"
                },
                {
                    "drm", true, "Draftsman"
                },
                {
                    "drt", true, "Director"
                },
                {
                    "dsr", true, "Designer"
                },
                {
                    "dst", true, "Distributor"
                },
                {
                    "dtc", true, "Data contributor"
                },
                {
                    "dte", true, "Dedicatee"
                },
                {
                    "dtm", true, "Data manager"
                },
                {
                    "dgg", true, "Degree granting institution"
                },
                {
                    "wst", true, "Writer of supplementary textual content"
                }
            });

            migrationBuilder.CreateIndex("IX_DocumentRoles_Enabled", "DocumentRoles", "Enabled");

            migrationBuilder.CreateIndex("IX_DocumentRoles_Name", "DocumentRoles", "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("DocumentRoles");
    }
}