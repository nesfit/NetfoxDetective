using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using PostSharp.Aspects.Advices;

namespace Netfox.FrameworkAPI.Tests
{
    public static class PcapPath
    {
        private const string TESTING_DATA_DIR = "TestingData";

        #region Pcaps

        public enum Pcaps
        {
            appIdent_test,

            appIdent_test1,

            appIdent_test2,

            appIdent1,

            pcr20160415,

            pcr20160416,

            appIdent_large_merge,

            app_identification_dnsHttpTls_cap,

            app_identification_learn1_cap,

            app_identification_refSkype_cap,

            app_identification_streamSkypeHttpTls_cap,

            app_identification_testM1_cap,

            app_identification_testM2_cap,

            email_imap_smtp_collector_selected_tcp_cap,

            email_pop_smtp_1_cap,

            email_pop_smtp_1_filtered_cap,

            email_test_imap_1_cap,

            email_test_imap_1_nm_cap,

            features_3frames_cap,

            features_PUSH_cap,

            features_putty_cap,

            features_putty_one_frame_cap,

            features_putty_select_cap,

            features_SSH_cap,

            features_sSSH_cap,

            features_SYN_cap,

            features_three_conver_putty_ssh_cap,

            GRE_ip_gre_ip_icmp_cap,

            http_caps_http_all_cap,

            http_caps_http_pc1_conversation_cap,

            http_caps_http_pc2_conversation_cap,

            http_caps_http_pc3_conversation_cap,

            http_caps_http_pc4_conversation_cap,

            pcap_mix_50_cap,

            pcap_mix_all_export_mix_cap,

            pcap_mix_http_cap,

            pcap_mix_icq1_cap,

            pcap_mix_msn_all_cap,

            pcap_mix_pg_cap,

            pcap_mix_root_cz_cap,

            pcap_mix_SpuriousFin_cap,

            pcap_mix_tcp_overflow_seq_mod_cap,

            pcap_mix_xmpp_nosecure_cap,

            pcap_mix_ymsg_without_file_cap,

            small_pcaps_0_cap,

            small_pcaps_ip_frag_3_cap,

            small_pcaps_malformed_last_packet_cap,

            small_pcaps_mnmfilter_cap,

            small_pcaps_tcp_reassembling_cap,

            small_pcaps_tcp_reassembling_more_missing_cap,

            small_pcaps_tcp_reassembling_one_missing_cap,

            voip_MLUVI_G723_G729_GSM_G711_cap,

            voip_rtp_cap,

            voip_rtp2_cap,

            dns_dns_sec_pcap,

            dns_dns_xvican01_01_pcap,

            dns_dns_xvican01_02_pcap,

            dns_dns_xvican01_03_pcap,

            email_imap_smtp_collector_pcap,

            email_imap_smtp_collector_filtered_pcap,

            email_imap_smtp_pc2_pcap,

            ftp_ftp_xkarpi03_01_pcap,

            ftp_ftp_xkarpi03_02_pcap,

            ftp_ftp_xkarpi03_03_upload_pcap,

            ftp_ftp_xkarpi03_04_delete_pcap,

            ftp_ftp_xkarpi03_05_download_pcap,

            ftp_ftp_xkarpi03_06_text_upload_pcap,

            GSE_UDP_DVB_S2_GSE_8_UPs_in_BB_frame_pcap,

            GSE_UDP_DVB_S2_GSE_complete_UP_2_pcap,

            GSE_UDP_DVB_S2_GSE_complete_UP_pcap,

            GSE_UDP_DVB_S2_GSE_fragmented_UP_pcap,

            GSE_UDP_DVB_S2_GSE_intermediate_UP_pcap,

            m57_m57_smtp_pcap,

            m57_m57_smtp_1_pcap,

            m57_m57_smtp_2_pcap,

            minecraft_xberan33_minecraft_at_port_27746_pcap,

            minecraft_xberan33_minecraft_at_port_27999_pcap,

            minecraft_xberan33_minecraft_at_port_37892_pcap,

            minecraft_xberan33_minecraft_at_port_41030_pcap,

            minecraft_xberan33_minecraft_bigger_pcap,

            minecraft_xberan33_minecraft_incorrect_pcap,

            minecraft_xberan33_minecraft_search_asd_pcap,

            minecraft_xberan33_minecraft_server_2_pcap,

            minecraft_xberan33_minecraft_server_3_pcap,

            minecraft_xberan33_minecraft_tell_cs_pcap,

            other_ascending_bytes_pcap,

            pcap_mix_evidence_packet_analysis_pcap,
            
            pcap_mix_isa_filtered_smaller_then80B_pcap,

            pcap_mix_isa_http_pcap,

            pcap_mix_mix_pcap,

            sip_caps_cancelled_pcap,

            sip_caps_direct_call__outgoing___sip_pcap,

            sip_caps_h323sip_pcap,

            sip_caps_rejected_pcap,

            sip_caps_sip_incoming_pcap,

            sip_caps_sip_rtcp_pcap,

            sip_caps_sip_rtcp_small_pcap,

            sip_caps_sip_h323_matej_pcap,

            sip_caps_sip_konference_na_asterisku_pcap,

            sip_caps_sip_no_inv_pcap,

            sip_caps_sip_no_ok_pcap,

            skalsky_dvur_aol_aol_pcap,

            skalsky_dvur_aol_ichat_pcap,

            skalsky_dvur_http_anonce_seznam_pcap,

            skalsky_dvur_http_chat_lide_cz_pcap,

            skalsky_dvur_http_seznam_hp_pcap,

            skalsky_dvur_http_seznam_reg_pcap,

            skalsky_dvur_rtp_rtp_example_pcap,

            skalsky_dvur_sip_aaa_pcap,

            skalsky_dvur_sip_SIP_CALL_RTP_G711_pcap,

            skalsky_dvur_sip_SIP_CALL_RTP_G711_RTP_ONLY_pcap,

            skalsky_dvur_tftp_tftp_rrq_pcap,

            skalsky_dvur_tftp_tftp_wrq_pcap,

            skalsky_dvur_ymsg_ymsg_56_pcap,

            small_pcaps_L2_missing_pcap,

            small_pcaps_malformed_last_packet_pcap,

            small_pcaps_tcp_fastretranssmit_pcap,

            small_pcaps_tcp_fragment_overlaps_old_data_pcap,

            voip_asterisk_koleje_test_call_gsm_pcap,

            voip_asterisk_koleje_test_call_pcap,

            voip_ekiga_netbox_to_cesnet_test_call_number_pcap,

            voip_h323_cisco_pcap,

            voip_h323sip_pcap,

            voip_martin_to_petr_gsm_pcap,

            voip_martin_to_petr_pcap,

            voip_martin_to_petr_petr_to_honza_gsm_pcap,

            voip_martin_nb_to_petr_pcap,

            voip_netbox_voip_gateway_to_ekiga_cesnet_pcap,

            voip_petr_to_honza_petr_to_martin_pcap,

            voip_petr_to_martin_pcap,

            voip_rtp_pcap,

            voip_rtp2_pcap,

            voip_sip_h323_isa_pcap,

            voip_sip_lab2_pcap,

            voip_sip_rtcp_pcap,

            voip_skinny_call_pcap,

            voip_skinny_registerl_pcap,

            voip_test2_2SIP_PCAP,

            voip_voice_2015_05_26_pcap,

            voip_cisco_g711alaw_pcap,

            voip_cisco_g711ulaw_pcap,

            voip_cisco_g729br8_pcap,

            voip_cisco_g729r8_pcap,

            voip_ekiga_amr_wb_pcap,

            voip_ekiga_g722_pcap,

            voip_ekiga_g7221_pcap,

            voip_ekiga_g726_16_pcap,

            voip_ekiga_g726_24_pcap,

            voip_ekiga_g726_32_pcap,

            voip_ekiga_g726_40_pcap,

            voip_ekiga_gsm_pcap,

            voip_ekiga_pcma_pcap,

            voip_ekiga_pcmu_pcap,

            voip_ekiga_silk16_pcap,

            voip_ekiga_silk8_pcap,

            voip_ekiga_speex16_pcap,

            voip_ekiga_speex8_pcap,

            voip_ixia_baapi_a_alaw_pcap,

            voip_ixia_baapi_a_amr_12_pcap,

            voip_ixia_baapi_a_g7231_5k_pcap,

            voip_ixia_baapi_a_g7231_6k_pcap,

            voip_ixia_baapi_a_g726_16_pcap,

            voip_ixia_baapi_a_g726_24_pcap,

            voip_ixia_baapi_a_g726_32_pcap,

            voip_ixia_baapi_a_g726_40_pcap,

            voip_ixia_baapi_a_g729_pcap,

            voip_ixia_baapi_a_g729a_pcap,

            voip_ixia_baapi_a_g729b_pcap,

            voip_ixia_baapi_a_ulaw_pcap,

            voip_ixia_babgk_a_alaw_pcap,

            voip_ixia_babgk_a_amr_12_pcap,

            voip_ixia_babgk_a_g7231_5k_pcap,

            voip_ixia_babgk_a_g7231_6k_pcap,

            voip_ixia_babgk_a_g726_16_pcap,

            voip_ixia_babgk_a_g726_24_pcap,

            voip_ixia_babgk_a_g726_32_pcap,

            voip_ixia_babgk_a_g726_40_pcap,

            voip_ixia_babgk_a_g729_pcap,

            voip_ixia_babgk_a_g729a_pcap,

            voip_ixia_babgk_a_g729b_pcap,

            voip_ixia_babgk_a_ulaw_pcap,

            whatsapp_whatsapp_register_and_call_1_5_pcap,

            whatsapp_wh_1_6_pcap,

            bitcoin_btc1_pcapng,

            bitcoin_btc1_part1_pcapng,

            bitcoin_btc2_snapshot_pcapng,

            bitcoin_btc2_pcapng,

            bitcoin_btc2_part1_pcapng,

            bitcoin_btc2_part2_pcapng,

            bitcoin_btc2_tx_01_pcapng,

            facebook_fb_chat_pcapng,

            facebook_fb_comment_pcapng,

            facebook_fb_file_pcapng,

            facebook_fb_group_pcapng,

            facebook_fb_groupfile_pcapng,

            facebook_fb_groupphoto_pcapng,

            facebook_fb_merged_pcapng,

            facebook_fb_photo_pcapng,

            facebook_fb_status_pcapng,

            hangouts_hangouts1_pcapng,

            http_caps_malyweb_pcapng,

            http_caps_malyweb2_pcapng,

            http_caps_malyweb3_pcapng,

            lide_lide_discussion_loaded_chat_pcapng,

            lide_lide_discussion_realtime_chat_pcapng,

            lide_lide_private_loaded_chat_pcapng,

            lide_lide_private_realtime_chat_pcapng,

            live_mail_centrum2_pcapng,

            live_mail_klikni_pcapng,

            live_mail_s9_pcapng,

            live_mail_seznalive4_pcapng,

            live_mail_seznam5_pcapng,

            live_mail_t3_pcapng,

            live_mail_t4_pcapng,

            live_mail_tiscali_pcapng,

            live_mail_tiscali2_pcapng,

            messenger_messenger1_pcapng,

            messenger_messenger2_pcapng,

            pcap_mix_centrum2_pcapng,

            pcap_mix_klikni_pcapng,

            pcap_mix_mixed_pdu_pcapng,

            pcap_mix_s9_pcapng,

            pcap_mix_seznalive4_pcapng,

            pcap_mix_seznam5_pcapng,

            pcap_mix_t3_pcapng,

            pcap_mix_t4_pcapng,

            pcap_mix_tcp_retr1_pcapng,

            pcap_mix_tiscali_pcapng,

            pcap_mix_tiscali2_pcapng,

            pcap_mix_udp_pcapng,

            pcap_mix_udp_tcp_pcapng,

            small_pcaps_tcp_more_frames_pcapng,

            small_pcaps_tcp_two_frames_pcapng,

            spdy_twit6_spdystreams_pcapng,

            twitter_twitter1_pcapng,

            twitter_twitter2_pcapng,

            webmail_webmail_gmail_pcapng,

            webmail_webmail_live_pcapng,

            webmail_webmail_live_test_pcapng,

            webmail_webmail_seznam_pcapng,

            webmail_webmail_yahoo_pcapng,

            webmail_webmail_yahoo_rc4_pcapng,

            xchat_xchat_chat_pcapng,

            xchat_xchat_room_chat_pcapng
        }

        private static readonly Dictionary<Pcaps, string> _pcapPaths = new Dictionary<Pcaps, string>
        {
            {Pcaps.appIdent_test, "large_pcaps\\appIdent_test.cap"},

            {Pcaps.appIdent_test1, "large_pcaps\\appIdent_test1.cap"},

            {Pcaps.appIdent_test2, "large_pcaps\\appIdent_test2.cap"},

            {Pcaps.appIdent1, "large_pcaps\\appIdent1.cap"},

            {Pcaps.pcr20160415, "large_pcaps\\pcr-20160415.cap"},

            {Pcaps.pcr20160416, "large_pcaps\\pcr-20160416.cap"},

            {Pcaps.appIdent_large_merge, "large_pcaps\\merge.cap"},

            {Pcaps.app_identification_dnsHttpTls_cap, "app_identification\\dnsHttpTls.cap"},

            {Pcaps.app_identification_learn1_cap, "app_identification\\learn1.cap"},

            {Pcaps.app_identification_refSkype_cap, "app_identification\\refSkype.cap"},

            {Pcaps.app_identification_streamSkypeHttpTls_cap, "app_identification\\streamSkypeHttpTls.cap"},

            {Pcaps.app_identification_testM1_cap, "app_identification\\testM1.cap"},

            {Pcaps.app_identification_testM2_cap, "app_identification\\testM2.cap"},

            {Pcaps.email_imap_smtp_collector_selected_tcp_cap, "email\\imap_smtp_collector_selected_tcp.cap"},

            {Pcaps.email_pop_smtp_1_cap, "email\\pop_smtp_1.cap"},

            {Pcaps.email_pop_smtp_1_filtered_cap, "email\\pop_smtp_1_filtered.cap"},

            {Pcaps.email_test_imap_1_cap, "email\\test_imap_1.cap"},

            {Pcaps.email_test_imap_1_nm_cap, "email\\test_imap_1_nm.cap"},

            {Pcaps.features_3frames_cap, "features\\3frames.cap"},

            {Pcaps.features_PUSH_cap, "features\\PUSH.cap"},

            {Pcaps.features_putty_cap, "features\\putty.cap"},

            {Pcaps.features_putty_one_frame_cap, "features\\putty_one_frame.cap"},

            {Pcaps.features_putty_select_cap, "features\\putty_select.cap"},

            {Pcaps.features_SSH_cap, "features\\SSH.cap"},

            {Pcaps.features_sSSH_cap, "features\\sSSH.cap"},

            {Pcaps.features_SYN_cap, "features\\SYN.cap"},

            {Pcaps.features_three_conver_putty_ssh_cap, "features\\three_conver_putty_ssh.cap"},

            {Pcaps.GRE_ip_gre_ip_icmp_cap, "GRE\\ip.gre.ip.icmp.cap"},

            {Pcaps.http_caps_http_all_cap, "http_caps\\http_all.cap"},

            {Pcaps.http_caps_http_pc1_conversation_cap, "http_caps\\http_pc1_conversation.cap"},

            {Pcaps.http_caps_http_pc2_conversation_cap, "http_caps\\http_pc2_conversation.cap"},

            {Pcaps.http_caps_http_pc3_conversation_cap, "http_caps\\http_pc3_conversation.cap"},

            {Pcaps.http_caps_http_pc4_conversation_cap, "http_caps\\http_pc4_conversation.cap"},

            {Pcaps.pcap_mix_50_cap, "pcap_mix\\50.cap"},

            {Pcaps.pcap_mix_all_export_mix_cap, "pcap_mix\\all_export_mix.cap"},

            {Pcaps.pcap_mix_http_cap, "pcap_mix\\http.cap"},

            {Pcaps.pcap_mix_icq1_cap, "pcap_mix\\icq1.cap"},

            {Pcaps.pcap_mix_msn_all_cap, "pcap_mix\\msn_all.cap"},

            {Pcaps.pcap_mix_pg_cap, "pcap_mix\\pg.cap"},

            {Pcaps.pcap_mix_root_cz_cap, "pcap_mix\\root_cz.cap"},

            {Pcaps.pcap_mix_SpuriousFin_cap, "pcap_mix\\SpuriousFin.cap"},

            {Pcaps.pcap_mix_tcp_overflow_seq_mod_cap, "pcap_mix\\tcp_overflow_seq_mod.cap"},

            {Pcaps.pcap_mix_xmpp_nosecure_cap, "pcap_mix\\xmpp_nosecure.cap"},

            {Pcaps.pcap_mix_ymsg_without_file_cap, "pcap_mix\\ymsg_without_file.cap"},

            {Pcaps.small_pcaps_0_cap, "small_pcaps\\0.cap"},

            {Pcaps.small_pcaps_ip_frag_3_cap, "small_pcaps\\ip_frag_3.cap"},

            {Pcaps.small_pcaps_malformed_last_packet_cap, "small_pcaps\\malformed_last_packet.cap"},

            {Pcaps.small_pcaps_mnmfilter_cap, "small_pcaps\\mnmfilter.cap"},

            {Pcaps.small_pcaps_tcp_reassembling_cap, "small_pcaps\\tcp_reassembling.cap"},

            {Pcaps.small_pcaps_tcp_reassembling_more_missing_cap, "small_pcaps\\tcp_reassembling_more_missing.cap"},

            {Pcaps.small_pcaps_tcp_reassembling_one_missing_cap, "small_pcaps\\tcp_reassembling_one_missing.cap"},

            {Pcaps.voip_MLUVI_G723_G729_GSM_G711_cap, "voip\\MLUVI_G723_G729_GSM_G711.cap"},

            {Pcaps.voip_rtp_cap, "voip\\rtp.cap"},

            {Pcaps.voip_rtp2_cap, "voip\\rtp2.cap"},

            {Pcaps.dns_dns_sec_pcap, "dns\\dns_sec.pcap"},

            {Pcaps.dns_dns_xvican01_01_pcap, "dns\\dns_xvican01_01.pcap"},

            {Pcaps.dns_dns_xvican01_02_pcap, "dns\\dns_xvican01_02.pcap"},

            {Pcaps.dns_dns_xvican01_03_pcap, "dns\\dns_xvican01_03.pcap"},

            {Pcaps.email_imap_smtp_collector_pcap, "email\\imap_smtp_collector.pcap"},

            {Pcaps.email_imap_smtp_collector_filtered_pcap, "email\\imap_smtp_collector_filtered.pcap"},

            {Pcaps.email_imap_smtp_pc2_pcap, "email\\imap_smtp_pc2.pcap"},

            {Pcaps.ftp_ftp_xkarpi03_01_pcap, "ftp\\ftp_xkarpi03_01.pcap"},

            {Pcaps.ftp_ftp_xkarpi03_02_pcap, "ftp\\ftp_xkarpi03_02.pcap"},

            {Pcaps.ftp_ftp_xkarpi03_03_upload_pcap, "ftp\\ftp_xkarpi03_03_upload.pcap"},

            {Pcaps.ftp_ftp_xkarpi03_04_delete_pcap, "ftp\\ftp_xkarpi03_04_delete.pcap"},

            {Pcaps.ftp_ftp_xkarpi03_05_download_pcap, "ftp\\ftp_xkarpi03_05_download.pcap"},

            {Pcaps.ftp_ftp_xkarpi03_06_text_upload_pcap, "ftp\\ftp_xkarpi03_06_text_upload.pcap"},

            {Pcaps.GSE_UDP_DVB_S2_GSE_8_UPs_in_BB_frame_pcap, "GSE\\UDP DVB-S2 GSE 8 UPs in BB frame.pcap"},

            {Pcaps.GSE_UDP_DVB_S2_GSE_complete_UP_2_pcap, "GSE\\UDP DVB-S2 GSE complete UP 2.pcap"},

            {Pcaps.GSE_UDP_DVB_S2_GSE_complete_UP_pcap, "GSE\\UDP DVB-S2 GSE complete UP.pcap"},

            {Pcaps.GSE_UDP_DVB_S2_GSE_fragmented_UP_pcap, "GSE\\UDP DVB-S2 GSE fragmented UP.pcap"},

            {Pcaps.GSE_UDP_DVB_S2_GSE_intermediate_UP_pcap, "GSE\\UDP DVB-S2 GSE intermediate UP.pcap"},

            {Pcaps.m57_m57_smtp_pcap, "m57\\m57_smtp.pcap"},

            {Pcaps.m57_m57_smtp_1_pcap, "m57\\m57_smtp_1.pcap"},

            {Pcaps.m57_m57_smtp_2_pcap, "m57\\m57_smtp_2.pcap"},

            {
                Pcaps.minecraft_xberan33_minecraft_at_port_27746_pcap,
                "minecraft\\xberan33_minecraft_at_port_27746.pcap"
            },

            {
                Pcaps.minecraft_xberan33_minecraft_at_port_27999_pcap,
                "minecraft\\xberan33_minecraft_at_port_27999.pcap"
            },

            {
                Pcaps.minecraft_xberan33_minecraft_at_port_37892_pcap,
                "minecraft\\xberan33_minecraft_at_port_37892.pcap"
            },

            {
                Pcaps.minecraft_xberan33_minecraft_at_port_41030_pcap,
                "minecraft\\xberan33_minecraft_at_port_41030.pcap"
            },

            {Pcaps.minecraft_xberan33_minecraft_bigger_pcap, "minecraft\\xberan33_minecraft_bigger.pcap"},

            {Pcaps.minecraft_xberan33_minecraft_incorrect_pcap, "minecraft\\xberan33_minecraft_incorrect.pcap"},

            {Pcaps.minecraft_xberan33_minecraft_search_asd_pcap, "minecraft\\xberan33_minecraft_search_asd.pcap"},

            {Pcaps.minecraft_xberan33_minecraft_server_2_pcap, "minecraft\\xberan33_minecraft_server_2.pcap"},

            {Pcaps.minecraft_xberan33_minecraft_server_3_pcap, "minecraft\\xberan33_minecraft_server_3.pcap"},

            {Pcaps.minecraft_xberan33_minecraft_tell_cs_pcap, "minecraft\\xberan33_minecraft_tell_cs.pcap"},

            {Pcaps.other_ascending_bytes_pcap, "other\\ascending_bytes.pcap"},

            {Pcaps.pcap_mix_evidence_packet_analysis_pcap, "pcap_mix\\evidence-packet-analysis.pcap"},

            {Pcaps.pcap_mix_isa_filtered_smaller_then80B_pcap, "pcap_mix\\isa-filtered-smaller-then80B.pcap"},

            {Pcaps.pcap_mix_isa_http_pcap, "pcap_mix\\isa-http.pcap"},

            {Pcaps.pcap_mix_mix_pcap, "pcap_mix\\mix.pcap"},

            {Pcaps.sip_caps_cancelled_pcap, "sip_caps\\cancelled.pcap"},

            {Pcaps.sip_caps_direct_call__outgoing___sip_pcap, "sip_caps\\direct call, outgoing - sip.pcap"},

            {Pcaps.sip_caps_h323sip_pcap, "sip_caps\\h323sip.pcap"},

            {Pcaps.sip_caps_rejected_pcap, "sip_caps\\rejected.pcap"},

            {Pcaps.sip_caps_sip_incoming_pcap, "sip_caps\\sip,incoming.pcap"},

            {Pcaps.sip_caps_sip_rtcp_pcap, "sip_caps\\sip-rtcp.pcap"},

            {Pcaps.sip_caps_sip_rtcp_small_pcap, "sip_caps\\sip-rtcp_small.pcap"},

            {Pcaps.sip_caps_sip_h323_matej_pcap, "sip_caps\\sip_h323-matej.pcap"},

            {Pcaps.sip_caps_sip_konference_na_asterisku_pcap, "sip_caps\\sip_konference_na_asterisku.pcap"},

            {Pcaps.sip_caps_sip_no_inv_pcap, "sip_caps\\sip_no_inv.pcap"},

            {Pcaps.sip_caps_sip_no_ok_pcap, "sip_caps\\sip_no_ok.pcap"},

            {Pcaps.skalsky_dvur_aol_aol_pcap, "skalsky_dvur\\aol\\aol.pcap"},

            {Pcaps.skalsky_dvur_aol_ichat_pcap, "skalsky_dvur\\aol\\ichat.pcap"},

            {Pcaps.skalsky_dvur_http_anonce_seznam_pcap, "skalsky_dvur\\http\\anonce_seznam.pcap"},

            {Pcaps.skalsky_dvur_http_chat_lide_cz_pcap, "skalsky_dvur\\http\\chat-lide-cz.pcap"},

            {Pcaps.skalsky_dvur_http_seznam_hp_pcap, "skalsky_dvur\\http\\seznam_hp.pcap"},

            {Pcaps.skalsky_dvur_http_seznam_reg_pcap, "skalsky_dvur\\http\\seznam_reg.pcap"},

            {Pcaps.skalsky_dvur_rtp_rtp_example_pcap, "skalsky_dvur\\rtp\\rtp_example.pcap"},

            {Pcaps.skalsky_dvur_sip_aaa_pcap, "skalsky_dvur\\sip\\aaa.pcap"},

            {Pcaps.skalsky_dvur_sip_SIP_CALL_RTP_G711_pcap, "skalsky_dvur\\sip\\SIP_CALL_RTP_G711.pcap"},

            {
                Pcaps.skalsky_dvur_sip_SIP_CALL_RTP_G711_RTP_ONLY_pcap,
                "skalsky_dvur\\sip\\SIP_CALL_RTP_G711_RTP_ONLY.pcap"
            },

            {Pcaps.skalsky_dvur_tftp_tftp_rrq_pcap, "skalsky_dvur\\tftp\\tftp_rrq.pcap"},

            {Pcaps.skalsky_dvur_tftp_tftp_wrq_pcap, "skalsky_dvur\\tftp\\tftp_wrq.pcap"},

            {Pcaps.skalsky_dvur_ymsg_ymsg_56_pcap, "skalsky_dvur\\ymsg\\ymsg_56.pcap"},

            {Pcaps.small_pcaps_L2_missing_pcap, "small_pcaps\\L2_missing.pcap"},

            {Pcaps.small_pcaps_malformed_last_packet_pcap, "small_pcaps\\malformed_last_packet.pcap"},

            {Pcaps.small_pcaps_tcp_fastretranssmit_pcap, "small_pcaps\\tcp_fastretranssmit.pcap"},

            {
                Pcaps.small_pcaps_tcp_fragment_overlaps_old_data_pcap,
                "small_pcaps\\tcp_fragment_overlaps_old_data.pcap"
            },

            {Pcaps.voip_asterisk_koleje_test_call_gsm_pcap, "voip\\asterisk-koleje-test-call-gsm.pcap"},

            {Pcaps.voip_asterisk_koleje_test_call_pcap, "voip\\asterisk-koleje-test-call.pcap"},

            {
                Pcaps.voip_ekiga_netbox_to_cesnet_test_call_number_pcap,
                "voip\\ekiga_netbox-to-cesnet_test_call_number.pcap"
            },

            {Pcaps.voip_h323_cisco_pcap, "voip\\h323-cisco.pcap"},

            {Pcaps.voip_h323sip_pcap, "voip\\h323sip.pcap"},

            {Pcaps.voip_martin_to_petr_gsm_pcap, "voip\\martin-to-petr-gsm.pcap"},

            {Pcaps.voip_martin_to_petr_pcap, "voip\\martin-to-petr.pcap"},

            {Pcaps.voip_martin_to_petr_petr_to_honza_gsm_pcap, "voip\\martin-to-petr_petr-to-honza-gsm.pcap"},

            {Pcaps.voip_martin_nb_to_petr_pcap, "voip\\martin_nb-to-petr.pcap"},

            {Pcaps.voip_netbox_voip_gateway_to_ekiga_cesnet_pcap, "voip\\netbox-voip-gateway-to-ekiga_cesnet.pcap"},

            {Pcaps.voip_petr_to_honza_petr_to_martin_pcap, "voip\\petr-to-honza_petr-to-martin.pcap"},

            {Pcaps.voip_petr_to_martin_pcap, "voip\\petr-to-martin.pcap"},

            {Pcaps.voip_rtp_pcap, "voip\\rtp.pcap"},

            {Pcaps.voip_rtp2_pcap, "voip\\rtp2.pcap"},

            {Pcaps.voip_sip_h323_isa_pcap, "voip\\sip-h323-isa.pcap"},

            {Pcaps.voip_sip_lab2_pcap, "voip\\sip-lab2.pcap"},

            {Pcaps.voip_sip_rtcp_pcap, "voip\\sip-rtcp.pcap"},

            {Pcaps.voip_skinny_call_pcap, "voip\\skinny-call.pcap"},

            {Pcaps.voip_skinny_registerl_pcap, "voip\\skinny-registerl.pcap"},

            {Pcaps.voip_test2_2SIP_PCAP, "voip\\test2_2SIP.PCAP"},

            {Pcaps.voip_voice_2015_05_26_pcap, "voip\\voice-2015-05-26.pcap"},

            {Pcaps.voip_cisco_g711alaw_pcap, "voip\\cisco\\g711alaw.pcap"},

            {Pcaps.voip_cisco_g711ulaw_pcap, "voip\\cisco\\g711ulaw.pcap"},

            {Pcaps.voip_cisco_g729br8_pcap, "voip\\cisco\\g729br8.pcap"},

            {Pcaps.voip_cisco_g729r8_pcap, "voip\\cisco\\g729r8.pcap"},

            {Pcaps.voip_ekiga_amr_wb_pcap, "voip\\ekiga\\amr-wb.pcap"},

            {Pcaps.voip_ekiga_g722_pcap, "voip\\ekiga\\g722.pcap"},

            {Pcaps.voip_ekiga_g7221_pcap, "voip\\ekiga\\g7221.pcap"},

            {Pcaps.voip_ekiga_g726_16_pcap, "voip\\ekiga\\g726-16.pcap"},

            {Pcaps.voip_ekiga_g726_24_pcap, "voip\\ekiga\\g726-24.pcap"},

            {Pcaps.voip_ekiga_g726_32_pcap, "voip\\ekiga\\g726-32.pcap"},

            {Pcaps.voip_ekiga_g726_40_pcap, "voip\\ekiga\\g726-40.pcap"},

            {Pcaps.voip_ekiga_gsm_pcap, "voip\\ekiga\\gsm.pcap"},

            {Pcaps.voip_ekiga_pcma_pcap, "voip\\ekiga\\pcma.pcap"},

            {Pcaps.voip_ekiga_pcmu_pcap, "voip\\ekiga\\pcmu.pcap"},

            {Pcaps.voip_ekiga_silk16_pcap, "voip\\ekiga\\silk16.pcap"},

            {Pcaps.voip_ekiga_silk8_pcap, "voip\\ekiga\\silk8.pcap"},

            {Pcaps.voip_ekiga_speex16_pcap, "voip\\ekiga\\speex16.pcap"},

            {Pcaps.voip_ekiga_speex8_pcap, "voip\\ekiga\\speex8.pcap"},

            {Pcaps.voip_ixia_baapi_a_alaw_pcap, "voip\\ixia\\baapi-a\\alaw.pcap"},

            {Pcaps.voip_ixia_baapi_a_amr_12_pcap, "voip\\ixia\\baapi-a\\amr-12.pcap"},

            {Pcaps.voip_ixia_baapi_a_g7231_5k_pcap, "voip\\ixia\\baapi-a\\g7231-5k.pcap"},

            {Pcaps.voip_ixia_baapi_a_g7231_6k_pcap, "voip\\ixia\\baapi-a\\g7231-6k.pcap"},

            {Pcaps.voip_ixia_baapi_a_g726_16_pcap, "voip\\ixia\\baapi-a\\g726-16.pcap"},

            {Pcaps.voip_ixia_baapi_a_g726_24_pcap, "voip\\ixia\\baapi-a\\g726-24.pcap"},

            {Pcaps.voip_ixia_baapi_a_g726_32_pcap, "voip\\ixia\\baapi-a\\g726-32.pcap"},

            {Pcaps.voip_ixia_baapi_a_g726_40_pcap, "voip\\ixia\\baapi-a\\g726-40.pcap"},

            {Pcaps.voip_ixia_baapi_a_g729_pcap, "voip\\ixia\\baapi-a\\g729.pcap"},

            {Pcaps.voip_ixia_baapi_a_g729a_pcap, "voip\\ixia\\baapi-a\\g729a.pcap"},

            {Pcaps.voip_ixia_baapi_a_g729b_pcap, "voip\\ixia\\baapi-a\\g729b.pcap"},

            {Pcaps.voip_ixia_baapi_a_ulaw_pcap, "voip\\ixia\\baapi-a\\ulaw.pcap"},

            {Pcaps.voip_ixia_babgk_a_alaw_pcap, "voip\\ixia\\babgk-a\\alaw.pcap"},

            {Pcaps.voip_ixia_babgk_a_amr_12_pcap, "voip\\ixia\\babgk-a\\amr-12.pcap"},

            {Pcaps.voip_ixia_babgk_a_g7231_5k_pcap, "voip\\ixia\\babgk-a\\g7231-5k.pcap"},

            {Pcaps.voip_ixia_babgk_a_g7231_6k_pcap, "voip\\ixia\\babgk-a\\g7231-6k.pcap"},

            {Pcaps.voip_ixia_babgk_a_g726_16_pcap, "voip\\ixia\\babgk-a\\g726-16.pcap"},

            {Pcaps.voip_ixia_babgk_a_g726_24_pcap, "voip\\ixia\\babgk-a\\g726-24.pcap"},

            {Pcaps.voip_ixia_babgk_a_g726_32_pcap, "voip\\ixia\\babgk-a\\g726-32.pcap"},

            {Pcaps.voip_ixia_babgk_a_g726_40_pcap, "voip\\ixia\\babgk-a\\g726-40.pcap"},

            {Pcaps.voip_ixia_babgk_a_g729_pcap, "voip\\ixia\\babgk-a\\g729.pcap"},

            {Pcaps.voip_ixia_babgk_a_g729a_pcap, "voip\\ixia\\babgk-a\\g729a.pcap"},

            {Pcaps.voip_ixia_babgk_a_g729b_pcap, "voip\\ixia\\babgk-a\\g729b.pcap"},

            {Pcaps.voip_ixia_babgk_a_ulaw_pcap, "voip\\ixia\\babgk-a\\ulaw.pcap"},

            {Pcaps.whatsapp_whatsapp_register_and_call_1_5_pcap, "whatsapp\\whatsapp_register_and_call_1.5.pcap"},

            {Pcaps.whatsapp_wh_1_6_pcap, "whatsapp\\wh_1.6.pcap"},

            {Pcaps.bitcoin_btc1_pcapng, "bitcoin\\btc1.pcapng"},

            {Pcaps.bitcoin_btc1_part1_pcapng, "bitcoin\\btc1_part1.pcapng"},

            {Pcaps.bitcoin_btc2_snapshot_pcapng, "bitcoin\\btc2-snapshot.pcapng"},

            {Pcaps.bitcoin_btc2_pcapng, "bitcoin\\btc2.pcapng"},

            {Pcaps.bitcoin_btc2_part1_pcapng, "bitcoin\\btc2_part1.pcapng"},

            {Pcaps.bitcoin_btc2_part2_pcapng, "bitcoin\\btc2_part2.pcapng"},

            {Pcaps.bitcoin_btc2_tx_01_pcapng, "bitcoin\\btc2_tx_01.pcapng"},

            {Pcaps.facebook_fb_chat_pcapng, "facebook\\fb_chat.pcapng"},

            {Pcaps.facebook_fb_comment_pcapng, "facebook\\fb_comment.pcapng"},

            {Pcaps.facebook_fb_file_pcapng, "facebook\\fb_file.pcapng"},

            {Pcaps.facebook_fb_group_pcapng, "facebook\\fb_group.pcapng"},

            {Pcaps.facebook_fb_groupfile_pcapng, "facebook\\fb_groupfile.pcapng"},

            {Pcaps.facebook_fb_groupphoto_pcapng, "facebook\\fb_groupphoto.pcapng"},

            {Pcaps.facebook_fb_merged_pcapng, "facebook\\fb_merged.pcapng"},

            {Pcaps.facebook_fb_photo_pcapng, "facebook\\fb_photo.pcapng"},

            {Pcaps.facebook_fb_status_pcapng, "facebook\\fb_status.pcapng"},

            {Pcaps.hangouts_hangouts1_pcapng, "hangouts\\hangouts1.pcapng"},

            {Pcaps.http_caps_malyweb_pcapng, "http_caps\\malyweb.pcapng"},

            {Pcaps.http_caps_malyweb2_pcapng, "http_caps\\malyweb2.pcapng"},

            {Pcaps.http_caps_malyweb3_pcapng, "http_caps\\malyweb3.pcapng"},

            {Pcaps.lide_lide_discussion_loaded_chat_pcapng, "lide\\lide_discussion_loaded_chat.pcapng"},

            {Pcaps.lide_lide_discussion_realtime_chat_pcapng, "lide\\lide_discussion_realtime_chat.pcapng"},

            {Pcaps.lide_lide_private_loaded_chat_pcapng, "lide\\lide_private_loaded_chat.pcapng"},

            {Pcaps.lide_lide_private_realtime_chat_pcapng, "lide\\lide_private_realtime_chat.pcapng"},

            {Pcaps.live_mail_centrum2_pcapng, "live_mail\\centrum2.pcapng"},

            {Pcaps.live_mail_klikni_pcapng, "live_mail\\klikni.pcapng"},

            {Pcaps.live_mail_s9_pcapng, "live_mail\\s9.pcapng"},

            {Pcaps.live_mail_seznalive4_pcapng, "live_mail\\seznalive4.pcapng"},

            {Pcaps.live_mail_seznam5_pcapng, "live_mail\\seznam5.pcapng"},

            {Pcaps.live_mail_t3_pcapng, "live_mail\\t3.pcapng"},

            {Pcaps.live_mail_t4_pcapng, "live_mail\\t4.pcapng"},

            {Pcaps.live_mail_tiscali_pcapng, "live_mail\\tiscali.pcapng"},

            {Pcaps.live_mail_tiscali2_pcapng, "live_mail\\tiscali2.pcapng"},

            {Pcaps.messenger_messenger1_pcapng, "messenger\\messenger1.pcapng"},

            {Pcaps.messenger_messenger2_pcapng, "messenger\\messenger2.pcapng"},

            {Pcaps.pcap_mix_centrum2_pcapng, "pcap_mix\\centrum2.pcapng"},

            {Pcaps.pcap_mix_klikni_pcapng, "pcap_mix\\klikni.pcapng"},

            {Pcaps.pcap_mix_mixed_pdu_pcapng, "pcap_mix\\mixed_pdu.pcapng"},

            {Pcaps.pcap_mix_s9_pcapng, "pcap_mix\\s9.pcapng"},

            {Pcaps.pcap_mix_seznalive4_pcapng, "pcap_mix\\seznalive4.pcapng"},

            {Pcaps.pcap_mix_seznam5_pcapng, "pcap_mix\\seznam5.pcapng"},

            {Pcaps.pcap_mix_t3_pcapng, "pcap_mix\\t3.pcapng"},

            {Pcaps.pcap_mix_t4_pcapng, "pcap_mix\\t4.pcapng"},

            {Pcaps.pcap_mix_tcp_retr1_pcapng, "pcap_mix\\tcp_retr1.pcapng"},

            {Pcaps.pcap_mix_tiscali_pcapng, "pcap_mix\\tiscali.pcapng"},

            {Pcaps.pcap_mix_tiscali2_pcapng, "pcap_mix\\tiscali2.pcapng"},

            {Pcaps.pcap_mix_udp_pcapng, "pcap_mix\\udp.pcapng"},

            {Pcaps.pcap_mix_udp_tcp_pcapng, "pcap_mix\\udp_tcp.pcapng"},

            {Pcaps.small_pcaps_tcp_more_frames_pcapng, "small_pcaps\\tcp_more_frames.pcapng"},

            {Pcaps.small_pcaps_tcp_two_frames_pcapng, "small_pcaps\\tcp_two_frames.pcapng"},

            {Pcaps.spdy_twit6_spdystreams_pcapng, "spdy\\twit6_spdystreams.pcapng"},

            {Pcaps.twitter_twitter1_pcapng, "twitter\\twitter1.pcapng"},

            {Pcaps.twitter_twitter2_pcapng, "twitter\\twitter2.pcapng"},

            {Pcaps.webmail_webmail_gmail_pcapng, "webmail\\webmail_gmail.pcapng"},

            {Pcaps.webmail_webmail_live_pcapng, "webmail\\webmail_live.pcapng"},

            {Pcaps.webmail_webmail_live_test_pcapng, "webmail\\webmail_live_test.pcapng"},

            {Pcaps.webmail_webmail_seznam_pcapng, "webmail\\webmail_seznam.pcapng"},

            {Pcaps.webmail_webmail_yahoo_pcapng, "webmail\\webmail_yahoo.pcapng"},

            {Pcaps.webmail_webmail_yahoo_rc4_pcapng, "webmail\\webmail_yahoo_rc4.pcapng"},

            {Pcaps.xchat_xchat_chat_pcapng, "xchat\\xchat_chat.pcapng"},

            {Pcaps.xchat_xchat_room_chat_pcapng, "xchat\\xchat_room_chat.pcapng"},
        };

        #endregion

        #region Keys

        public enum KeyPath
        {
            pk_pem,
            fb_pk,
            lide_pk,
            fw_pk_pem
        }

        private static readonly Dictionary<KeyPath, string> _keys = new Dictionary<KeyPath, string>
        {
            {KeyPath.pk_pem, "webmail\\pk.pem"},
            {KeyPath.fb_pk, "facebook\\fb_pk.pem"},
            {KeyPath.lide_pk, "lide\\lide_pk.pem"},
            {KeyPath.fw_pk_pem, "webmail\\pk.pem"}
        };

        #endregion
        
        #region WDat

        public enum Wdat
        {
            warcraft_xberan33_1_wdat,

            warcraft_xberan33_2_wdat,

            warcraft_xberan33_3_wdat,

            warcraft_xberan33_bnet_whisper_wdat,

            warcraft_xberan33_channel_wdat,

            warcraft_xberan33_guild_wdat,

            warcraft_xberan33_instance_wdat,

            warcraft_xberan33_instance_leader_wdat,

            warcraft_xberan33_party_wdat,

            warcraft_xberan33_party_leader_wdat,

            warcraft_xberan33_raid_wdat,

            warcraft_xberan33_raid_leader_wdat,

            warcraft_xberan33_raid_warning_wdat,

            warcraft_xberan33_say_yell_wdat,

            warcraft_xberan33_whisper_wdat
        }

        private static readonly Dictionary<Wdat, string> _wdatPaths = new Dictionary<Wdat, string>
        {
            {Wdat.warcraft_xberan33_1_wdat, "warcraft\\xberan33_1.wdat"},
            {Wdat.warcraft_xberan33_2_wdat, "warcraft\\xberan33_2.wdat"},
            {Wdat.warcraft_xberan33_3_wdat, "warcraft\\xberan33_3.wdat"},
            {Wdat.warcraft_xberan33_bnet_whisper_wdat, "warcraft\\xberan33_bnet_whisper.wdat"},
            {Wdat.warcraft_xberan33_channel_wdat, "warcraft\\xberan33_channel.wdat"},
            {Wdat.warcraft_xberan33_guild_wdat, "warcraft\\xberan33_guild.wdat"},
            {Wdat.warcraft_xberan33_instance_wdat, "warcraft\\xberan33_instance.wdat"},
            {Wdat.warcraft_xberan33_instance_leader_wdat, "warcraft\\xberan33_instance_leader.wdat"},
            {Wdat.warcraft_xberan33_party_wdat, "warcraft\\xberan33_party.wdat"},
            {Wdat.warcraft_xberan33_party_leader_wdat, "warcraft\\xberan33_party_leader.wdat"},
            {Wdat.warcraft_xberan33_raid_wdat, "warcraft\\xberan33_raid.wdat"},
            {Wdat.warcraft_xberan33_raid_leader_wdat, "warcraft\\xberan33_raid_leader.wdat"},
            {Wdat.warcraft_xberan33_raid_warning_wdat, "warcraft\\xberan33_raid_warning.wdat"},
            {Wdat.warcraft_xberan33_say_yell_wdat, "warcraft\\xberan33_say_yell.wdat"},
            {Wdat.warcraft_xberan33_whisper_wdat, "warcraft\\xberan33_whisper.wdat"}
        };
        
        #endregion

        public static string GetPcap(Pcaps pcap)
        {
            if (!_pcapPaths.ContainsKey(pcap))
                throw new KeyNotFoundException($"Pcap \"{pcap}\" not found in PCAP dictionary");

            return Path.GetFullPath(Path.Combine(GetTestingDir(), _pcapPaths[pcap]));
        }

        public static string GetKey(KeyPath key)
        {
            if (!_keys.ContainsKey(key))
                throw new KeyNotFoundException($"Key \"{key}\" not found in keys dictionary");

            return Path.GetFullPath(Path.Combine(GetTestingDir(), _keys[key]));
        }

        public static string GetWdat(Wdat wdat)
        {
            if (!_wdatPaths.ContainsKey(wdat))
                throw new KeyNotFoundException($"Wdat \"{wdat}\" not found in WDat dictionary");

            return Path.GetFullPath(Path.Combine(GetTestingDir(), _wdatPaths[wdat]));
        }

        private static string GetTestingDir()
        {
            string path = Directory.GetCurrentDirectory();
            while (!Directory.Exists(Path.Combine(path, TESTING_DATA_DIR)))
                path = Path.GetDirectoryName(path);

            return Path.Combine(path, TESTING_DATA_DIR);
        }
    }
}