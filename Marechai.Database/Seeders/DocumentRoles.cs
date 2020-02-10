using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Database.Seeders
{
    public static class DocumentRoles
    {
        public static void Seed(ModelBuilder modelBuilder) => modelBuilder.Entity<DocumentRole>().HasData(new
        {
            Id = "abr", Name = "Abridger", Enabled = true
        }, new
        {
            Id = "acp", Name = "Art copyist", Enabled = true
        }, new
        {
            Id = "act", Name = "Actor", Enabled = true
        }, new
        {
            Id = "adi", Name = "Art director", Enabled = true
        }, new
        {
            Id = "adp", Name = "Adapter", Enabled = true
        }, new
        {
            Id = "aft", Name = "Author of afterword, colophon, etc.", Enabled = true
        }, new
        {
            Id = "anl", Name = "Analyst", Enabled = true
        }, new
        {
            Id = "anm", Name = "Animator", Enabled = true
        }, new
        {
            Id = "ann", Name = "Annotator", Enabled = true
        }, new
        {
            Id = "ant", Name = "Bibliographic antecedent", Enabled = true
        }, new
        {
            Id = "ape", Name = "Appellee", Enabled = true
        }, new
        {
            Id = "apl", Name = "Appellant", Enabled = true
        }, new
        {
            Id = "app", Name = "Applicant", Enabled = true
        }, new
        {
            Id = "aqt", Name = "Author in quotations or text abstracts", Enabled = true
        }, new
        {
            Id = "arc", Name = "Architect", Enabled = true
        }, new
        {
            Id = "ard", Name = "Artistic director", Enabled = true
        }, new
        {
            Id = "arr", Name = "Arranger", Enabled = true
        }, new
        {
            Id = "art", Name = "Artist", Enabled = true
        }, new
        {
            Id = "asg", Name = "Assignee", Enabled = true
        }, new
        {
            Id = "asn", Name = "Associated name", Enabled = true
        }, new
        {
            Id = "ato", Name = "Autographer", Enabled = true
        }, new
        {
            Id = "att", Name = "Attributed name", Enabled = true
        }, new
        {
            Id = "auc", Name = "Auctioneer", Enabled = true
        }, new
        {
            Id = "aud", Name = "Author of dialog", Enabled = true
        }, new
        {
            Id = "aui", Name = "Author of introduction, etc.", Enabled = true
        }, new
        {
            Id = "aus", Name = "Screenwriter", Enabled = true
        }, new
        {
            Id = "aut", Name = "Author", Enabled = true
        }, new
        {
            Id = "bdd", Name = "Binding designer", Enabled = true
        }, new
        {
            Id = "bjd", Name = "Bookjacket designer", Enabled = true
        }, new
        {
            Id = "bkd", Name = "Book designer", Enabled = true
        }, new
        {
            Id = "bkp", Name = "Book producer", Enabled = true
        }, new
        {
            Id = "blw", Name = "Blurb writer", Enabled = true
        }, new
        {
            Id = "bnd", Name = "Binder", Enabled = true
        }, new
        {
            Id = "bpd", Name = "Bookplate designer", Enabled = true
        }, new
        {
            Id = "brd", Name = "Broadcaster", Enabled = true
        }, new
        {
            Id = "brl", Name = "Braille embosser", Enabled = true
        }, new
        {
            Id = "bsl", Name = "Bookseller", Enabled = true
        }, new
        {
            Id = "cas", Name = "Caster", Enabled = true
        }, new
        {
            Id = "ccp", Name = "Conceptor", Enabled = true
        }, new
        {
            Id = "chr", Name = "Choreographer", Enabled = true
        }, new
        {
            Id = "clb", Name = "Collaborator", Enabled = false
        }, new
        {
            Id = "cli", Name = "Client", Enabled = true
        }, new
        {
            Id = "cll", Name = "Calligrapher", Enabled = true
        }, new
        {
            Id = "clr", Name = "Colorist", Enabled = true
        }, new
        {
            Id = "clt", Name = "Collotyper", Enabled = true
        }, new
        {
            Id = "cmm", Name = "Commentator", Enabled = true
        }, new
        {
            Id = "cmp", Name = "Composer", Enabled = true
        }, new
        {
            Id = "cmt", Name = "Compositor", Enabled = true
        }, new
        {
            Id = "cnd", Name = "Conductor", Enabled = true
        }, new
        {
            Id = "cng", Name = "Cinematographer", Enabled = true
        }, new
        {
            Id = "cns", Name = "Censor", Enabled = true
        }, new
        {
            Id = "coe", Name = "Contestant-appellee", Enabled = true
        }, new
        {
            Id = "col", Name = "Collector", Enabled = true
        }, new
        {
            Id = "com", Name = "Compiler", Enabled = true
        }, new
        {
            Id = "con", Name = "Conservator", Enabled = true
        }, new
        {
            Id = "cor", Name = "Collection registrar", Enabled = true
        }, new
        {
            Id = "cos", Name = "Contestant", Enabled = true
        }, new
        {
            Id = "cot", Name = "Contestant-appellant", Enabled = true
        }, new
        {
            Id = "cou", Name = "Court governed", Enabled = true
        }, new
        {
            Id = "cov", Name = "Cover designer", Enabled = true
        }, new
        {
            Id = "cpc", Name = "Copyright claimant", Enabled = true
        }, new
        {
            Id = "cpe", Name = "Complainant-appellee", Enabled = true
        }, new
        {
            Id = "cph", Name = "Copyright holder", Enabled = true
        }, new
        {
            Id = "cpl", Name = "Complainant", Enabled = true
        }, new
        {
            Id = "cpt", Name = "Complainant-appellant", Enabled = true
        }, new
        {
            Id = "cre", Name = "Creator", Enabled = true
        }, new
        {
            Id = "crp", Name = "Correspondent", Enabled = true
        }, new
        {
            Id = "crr", Name = "Corrector", Enabled = true
        }, new
        {
            Id = "crt", Name = "Court reporter", Enabled = true
        }, new
        {
            Id = "csl", Name = "Consultant", Enabled = true
        }, new
        {
            Id = "csp", Name = "Consultant to a project", Enabled = true
        }, new
        {
            Id = "cst", Name = "Costume designer", Enabled = true
        }, new
        {
            Id = "ctb", Name = "Contributor", Enabled = true
        }, new
        {
            Id = "cte", Name = "Contestee-appellee", Enabled = true
        }, new
        {
            Id = "ctg", Name = "Cartographer", Enabled = true
        }, new
        {
            Id = "ctr", Name = "Contractor", Enabled = true
        }, new
        {
            Id = "cts", Name = "Contestee", Enabled = true
        }, new
        {
            Id = "ctt", Name = "Contestee-appellant", Enabled = true
        }, new
        {
            Id = "cur", Name = "Curator", Enabled = true
        }, new
        {
            Id = "cwt", Name = "Commentator for written text", Enabled = true
        }, new
        {
            Id = "dbp", Name = "Distribution place", Enabled = true
        }, new
        {
            Id = "dfd", Name = "Defendant", Enabled = true
        }, new
        {
            Id = "dfe", Name = "Defendant-appellee", Enabled = true
        }, new
        {
            Id = "dft", Name = "Defendant-appellant", Enabled = true
        }, new
        {
            Id = "dgg", Name = "Degree granting institution", Enabled = true
        }, new
        {
            Id = "dgs", Name = "Degree supervisor", Enabled = true
        }, new
        {
            Id = "dis", Name = "Dissertant", Enabled = true
        }, new
        {
            Id = "dln", Name = "Delineator", Enabled = true
        }, new
        {
            Id = "dnc", Name = "Dancer", Enabled = true
        }, new
        {
            Id = "dnr", Name = "Donor", Enabled = true
        }, new
        {
            Id = "dpc", Name = "Depicted", Enabled = true
        }, new
        {
            Id = "dpt", Name = "Depositor", Enabled = true
        }, new
        {
            Id = "drm", Name = "Draftsman", Enabled = true
        }, new
        {
            Id = "drt", Name = "Director", Enabled = true
        }, new
        {
            Id = "dsr", Name = "Designer", Enabled = true
        }, new
        {
            Id = "dst", Name = "Distributor", Enabled = true
        }, new
        {
            Id = "dtc", Name = "Data contributor", Enabled = true
        }, new
        {
            Id = "dte", Name = "Dedicatee", Enabled = true
        }, new
        {
            Id = "dtm", Name = "Data manager", Enabled = true
        }, new
        {
            Id = "dto", Name = "Dedicator", Enabled = true
        }, new
        {
            Id = "dub", Name = "Dubious author", Enabled = true
        }, new
        {
            Id = "edc", Name = "Editor of compilation", Enabled = true
        }, new
        {
            Id = "edm", Name = "Editor of moving image work", Enabled = true
        }, new
        {
            Id = "edt", Name = "Editor", Enabled = true
        }, new
        {
            Id = "egr", Name = "Engraver", Enabled = true
        }, new
        {
            Id = "elg", Name = "Electrician", Enabled = true
        }, new
        {
            Id = "elt", Name = "Electrotyper", Enabled = true
        }, new
        {
            Id = "eng", Name = "Engineer", Enabled = true
        }, new
        {
            Id = "enj", Name = "Enacting jurisdiction", Enabled = true
        }, new
        {
            Id = "etr", Name = "Etcher", Enabled = true
        }, new
        {
            Id = "evp", Name = "Event place", Enabled = true
        }, new
        {
            Id = "exp", Name = "Expert", Enabled = true
        }, new
        {
            Id = "fac", Name = "Facsimilist", Enabled = true
        }, new
        {
            Id = "fds", Name = "Film distributor", Enabled = true
        }, new
        {
            Id = "fld", Name = "Field director", Enabled = true
        }, new
        {
            Id = "flm", Name = "Film editor", Enabled = true
        }, new
        {
            Id = "fmd", Name = "Film director", Enabled = true
        }, new
        {
            Id = "fmk", Name = "Filmmaker", Enabled = true
        }, new
        {
            Id = "fmo", Name = "Former owner", Enabled = true
        }, new
        {
            Id = "fmp", Name = "Film producer", Enabled = true
        }, new
        {
            Id = "fnd", Name = "Funder", Enabled = true
        }, new
        {
            Id = "fpy", Name = "First party", Enabled = true
        }, new
        {
            Id = "frg", Name = "Forger", Enabled = true
        }, new
        {
            Id = "gis", Name = "Geographic information specialist", Enabled = true
        }, new
        {
            Id = "grt", Name = "Graphic technician", Enabled = false
        }, new
        {
            Id = "his", Name = "Host institution", Enabled = true
        }, new
        {
            Id = "hnr", Name = "Honoree", Enabled = true
        }, new
        {
            Id = "hst", Name = "Host", Enabled = true
        }, new
        {
            Id = "ill", Name = "Illustrator", Enabled = true
        }, new
        {
            Id = "ilu", Name = "Illuminator", Enabled = true
        }, new
        {
            Id = "ins", Name = "Inscriber", Enabled = true
        }, new
        {
            Id = "inv", Name = "Inventor", Enabled = true
        }, new
        {
            Id = "isb", Name = "Issuing body", Enabled = true
        }, new
        {
            Id = "itr", Name = "Instrumentalist", Enabled = true
        }, new
        {
            Id = "ive", Name = "Interviewee", Enabled = true
        }, new
        {
            Id = "ivr", Name = "Interviewer", Enabled = true
        }, new
        {
            Id = "jud", Name = "Judge", Enabled = true
        }, new
        {
            Id = "jug", Name = "Jurisdiction governed", Enabled = true
        }, new
        {
            Id = "lbr", Name = "Laboratory", Enabled = true
        }, new
        {
            Id = "lbt", Name = "Librettist", Enabled = true
        }, new
        {
            Id = "ldr", Name = "Laboratory director", Enabled = true
        }, new
        {
            Id = "led", Name = "Lead", Enabled = true
        }, new
        {
            Id = "lee", Name = "Libelee-appellee", Enabled = true
        }, new
        {
            Id = "lel", Name = "Libelee", Enabled = true
        }, new
        {
            Id = "len", Name = "Lender", Enabled = true
        }, new
        {
            Id = "let", Name = "Libelee-appellant", Enabled = true
        }, new
        {
            Id = "lgd", Name = "Lighting designer", Enabled = true
        }, new
        {
            Id = "lie", Name = "Libelant-appellee", Enabled = true
        }, new
        {
            Id = "lil", Name = "Libelant", Enabled = true
        }, new
        {
            Id = "lit", Name = "Libelant-appellant", Enabled = true
        }, new
        {
            Id = "lsa", Name = "Landscape architect", Enabled = true
        }, new
        {
            Id = "lse", Name = "Licensee", Enabled = true
        }, new
        {
            Id = "lso", Name = "Licensor", Enabled = true
        }, new
        {
            Id = "ltg", Name = "Lithographer", Enabled = true
        }, new
        {
            Id = "lyr", Name = "Lyricist", Enabled = true
        }, new
        {
            Id = "mcp", Name = "Music copyist", Enabled = true
        }, new
        {
            Id = "mdc", Name = "Metadata contact", Enabled = true
        }, new
        {
            Id = "med", Name = "Medium", Enabled = true
        }, new
        {
            Id = "mfp", Name = "Manufacture place", Enabled = true
        }, new
        {
            Id = "mfr", Name = "Manufacturer", Enabled = true
        }, new
        {
            Id = "mod", Name = "Moderator", Enabled = true
        }, new
        {
            Id = "mon", Name = "Monitor", Enabled = true
        }, new
        {
            Id = "mrb", Name = "Marbler", Enabled = true
        }, new
        {
            Id = "mrk", Name = "Markup editor", Enabled = true
        }, new
        {
            Id = "msd", Name = "Musical director", Enabled = true
        }, new
        {
            Id = "mte", Name = "Metal-engraver", Enabled = true
        }, new
        {
            Id = "mtk", Name = "Minute taker", Enabled = true
        }, new
        {
            Id = "mus", Name = "Musician", Enabled = true
        }, new
        {
            Id = "nrt", Name = "Narrator", Enabled = true
        }, new
        {
            Id = "opn", Name = "Opponent", Enabled = true
        }, new
        {
            Id = "org", Name = "Originator", Enabled = true
        }, new
        {
            Id = "orm", Name = "Organizer", Enabled = true
        }, new
        {
            Id = "osp", Name = "Onscreen presenter", Enabled = true
        }, new
        {
            Id = "oth", Name = "Other", Enabled = true
        }, new
        {
            Id = "own", Name = "Owner", Enabled = true
        }, new
        {
            Id = "pan", Name = "Panelist", Enabled = true
        }, new
        {
            Id = "pat", Name = "Patron", Enabled = true
        }, new
        {
            Id = "pbd", Name = "Publishing director", Enabled = true
        }, new
        {
            Id = "pbl", Name = "Publisher", Enabled = true
        }, new
        {
            Id = "pdr", Name = "Project director", Enabled = true
        }, new
        {
            Id = "pfr", Name = "Proofreader", Enabled = true
        }, new
        {
            Id = "pht", Name = "Photographer", Enabled = true
        }, new
        {
            Id = "plt", Name = "Platemaker", Enabled = true
        }, new
        {
            Id = "pma", Name = "Permitting agency", Enabled = true
        }, new
        {
            Id = "pmn", Name = "Production manager", Enabled = true
        }, new
        {
            Id = "pop", Name = "Printer of plates", Enabled = true
        }, new
        {
            Id = "ppm", Name = "Papermaker", Enabled = true
        }, new
        {
            Id = "ppt", Name = "Puppeteer", Enabled = true
        }, new
        {
            Id = "pra", Name = "Praeses", Enabled = true
        }, new
        {
            Id = "prc", Name = "Process contact", Enabled = true
        }, new
        {
            Id = "prd", Name = "Production personnel", Enabled = true
        }, new
        {
            Id = "pre", Name = "Presenter", Enabled = true
        }, new
        {
            Id = "prf", Name = "Performer", Enabled = true
        }, new
        {
            Id = "prg", Name = "Programmer", Enabled = true
        }, new
        {
            Id = "prm", Name = "Printmaker", Enabled = true
        }, new
        {
            Id = "prn", Name = "Production company", Enabled = true
        }, new
        {
            Id = "pro", Name = "Producer", Enabled = true
        }, new
        {
            Id = "prp", Name = "Production place", Enabled = true
        }, new
        {
            Id = "prs", Name = "Production designer", Enabled = true
        }, new
        {
            Id = "prt", Name = "Printer", Enabled = true
        }, new
        {
            Id = "prv", Name = "Provider", Enabled = true
        }, new
        {
            Id = "pta", Name = "Patent applicant", Enabled = true
        }, new
        {
            Id = "pte", Name = "Plaintiff-appellee", Enabled = true
        }, new
        {
            Id = "ptf", Name = "Plaintiff", Enabled = true
        }, new
        {
            Id = "pth", Name = "Patent holder", Enabled = true
        }, new
        {
            Id = "ptt", Name = "Plaintiff-appellant", Enabled = true
        }, new
        {
            Id = "pup", Name = "Publication place", Enabled = true
        }, new
        {
            Id = "rbr", Name = "Rubricator", Enabled = true
        }, new
        {
            Id = "rcd", Name = "Recordist", Enabled = true
        }, new
        {
            Id = "rce", Name = "Recording engineer", Enabled = true
        }, new
        {
            Id = "rcp", Name = "Addressee", Enabled = true
        }, new
        {
            Id = "rdd", Name = "Radio director", Enabled = true
        }, new
        {
            Id = "red", Name = "Redaktor", Enabled = true
        }, new
        {
            Id = "ren", Name = "Renderer", Enabled = true
        }, new
        {
            Id = "res", Name = "Researcher", Enabled = true
        }, new
        {
            Id = "rev", Name = "Reviewer", Enabled = true
        }, new
        {
            Id = "rpc", Name = "Radio producer", Enabled = true
        }, new
        {
            Id = "rps", Name = "Repository", Enabled = true
        }, new
        {
            Id = "rpt", Name = "Reporter", Enabled = true
        }, new
        {
            Id = "rpy", Name = "Responsible party", Enabled = true
        }, new
        {
            Id = "rse", Name = "Respondent-appellee", Enabled = true
        }, new
        {
            Id = "rsg", Name = "Restager", Enabled = true
        }, new
        {
            Id = "rsp", Name = "Respondent", Enabled = true
        }, new
        {
            Id = "rsr", Name = "Restorationist", Enabled = true
        }, new
        {
            Id = "rst", Name = "Respondent-appellant", Enabled = true
        }, new
        {
            Id = "rth", Name = "Research team head", Enabled = true
        }, new
        {
            Id = "rtm", Name = "Research team member", Enabled = true
        }, new
        {
            Id = "sad", Name = "Scientific advisor", Enabled = true
        }, new
        {
            Id = "sce", Name = "Scenarist", Enabled = true
        }, new
        {
            Id = "scl", Name = "Sculptor", Enabled = true
        }, new
        {
            Id = "scr", Name = "Scribe", Enabled = true
        }, new
        {
            Id = "sds", Name = "Sound designer", Enabled = true
        }, new
        {
            Id = "sec", Name = "Secretary", Enabled = true
        }, new
        {
            Id = "sgd", Name = "Stage director", Enabled = true
        }, new
        {
            Id = "sgn", Name = "Signer", Enabled = true
        }, new
        {
            Id = "sht", Name = "Supporting host", Enabled = true
        }, new
        {
            Id = "sll", Name = "Seller", Enabled = true
        }, new
        {
            Id = "sng", Name = "Singer", Enabled = true
        }, new
        {
            Id = "spk", Name = "Speaker", Enabled = true
        }, new
        {
            Id = "spn", Name = "Sponsor", Enabled = true
        }, new
        {
            Id = "spy", Name = "Second party", Enabled = true
        }, new
        {
            Id = "srv", Name = "Surveyor", Enabled = true
        }, new
        {
            Id = "std", Name = "Set designer", Enabled = true
        }, new
        {
            Id = "stg", Name = "Setting", Enabled = true
        }, new
        {
            Id = "stl", Name = "Storyteller", Enabled = true
        }, new
        {
            Id = "stm", Name = "Stage manager", Enabled = true
        }, new
        {
            Id = "stn", Name = "Standards body", Enabled = true
        }, new
        {
            Id = "str", Name = "Stereotyper", Enabled = true
        }, new
        {
            Id = "tcd", Name = "Technical director", Enabled = true
        }, new
        {
            Id = "tch", Name = "Teacher", Enabled = true
        }, new
        {
            Id = "ths", Name = "Thesis advisor", Enabled = true
        }, new
        {
            Id = "tld", Name = "Television director", Enabled = true
        }, new
        {
            Id = "tlp", Name = "Television producer", Enabled = true
        }, new
        {
            Id = "trc", Name = "Transcriber", Enabled = true
        }, new
        {
            Id = "trl", Name = "Translator", Enabled = true
        }, new
        {
            Id = "tyd", Name = "Type designer", Enabled = true
        }, new
        {
            Id = "tyg", Name = "Typographer", Enabled = true
        }, new
        {
            Id = "uvp", Name = "University place", Enabled = true
        }, new
        {
            Id = "vac", Name = "Voice actor", Enabled = true
        }, new
        {
            Id = "vdg", Name = "Videographer", Enabled = true
        }, new
        {
            Id = "voc", Name = "Vocalist", Enabled = false
        }, new
        {
            Id = "wac", Name = "Writer of added commentary", Enabled = true
        }, new
        {
            Id = "wal", Name = "Writer of added lyrics", Enabled = true
        }, new
        {
            Id = "wam", Name = "Writer of accompanying material", Enabled = true
        }, new
        {
            Id = "wat", Name = "Writer of added text", Enabled = true
        }, new
        {
            Id = "wdc", Name = "Woodcutter", Enabled = true
        }, new
        {
            Id = "wde", Name = "Wood engraver", Enabled = true
        }, new
        {
            Id = "win", Name = "Writer of introduction", Enabled = true
        }, new
        {
            Id = "wit", Name = "Witness", Enabled = true
        }, new
        {
            Id = "wpr", Name = "Writer of preface", Enabled = true
        }, new
        {
            Id = "wst", Name = "Writer of supplementary textual content", Enabled = true
        });
    }
}